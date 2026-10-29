using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Extensions;
using Resto.Front.Api.Data.Organization.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Resto.Front.Api.HorecaControlPlugin
{
    /// <summary>
    /// Opt-in transport for Smart Horeca QR orders.
    /// IMPORTANT: this service NEVER accepts online payments, closes orders, or
    /// marks fiscal receipts paid. All creation requires the local opt-in flags.
    /// Existing event/Socket.IO flows remain unchanged.
    /// </summary>
    public sealed class QrPosBridgeService : IDisposable
    {
        private const string ExternalOrderKey = "sh.qr.external_order_id";
        private const string OriginName = "Smart Horeca QR";
        private readonly QrPosBridgeSettings settings;
        private readonly SocketIoConnectorConfig identity;
        private readonly HttpClient http;
        private readonly CancellationTokenSource stop = new CancellationTokenSource();
        private readonly object journalSync = new object();
        private readonly string journalDirectory;
        private Task runningTask;
        private bool disposed;

        public QrPosBridgeService(QrPosBridgeSettings settings, SocketIoConnectorConfig identity)
        {
            this.settings = settings ?? new QrPosBridgeSettings();
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false };
            http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(12) };
            journalDirectory = Path.Combine(PluginHelpers.StorageDirectory, "QrPosBridge");
        }

        public void Start()
        {
            if (!settings.Enabled)
            {
                PluginContext.Log.Info("QrPosBridge :: disabled (no changes to POS orders).");
                return;
            }
            if (!TryValidateConfiguration(out var reason))
            {
                PluginContext.Log.Warn("QrPosBridge :: disabled: " + reason);
                return;
            }
            Directory.CreateDirectory(journalDirectory);
            runningTask = Task.Run(() => RunAsync(stop.Token));
            PluginContext.Log.Info(settings.AllowOrderCreation
                ? "QrPosBridge :: opt-in processing started."
                : "QrPosBridge :: diagnostics only (no polling, POS orders or kitchen printing).");
        }

        private bool TryValidateConfiguration(out string reason)
        {
            reason = "";
            if (string.IsNullOrWhiteSpace(settings.PluginIngressToken) ||
                string.IsNullOrWhiteSpace(settings.BridgeToken))
            {
                reason = "QR bridge credentials are missing";
                return false;
            }
            // Diagnostics can authenticate the connection without POS permissions.
            // The dangerous full workflow requires explicit independent opt-ins.
            if (settings.AllowOrderCreation &&
                (string.IsNullOrWhiteSpace(settings.OperatorPin) || !settings.AllowKitchenPrint))
            {
                reason = "POS creation requires an operator PIN and kitchen-print opt-in";
                return false;
            }
            if (identity.PluginId == Guid.Empty || identity.DepartmentId == Guid.Empty)
            {
                reason = "invalid plugin/restaurant identity";
                return false;
            }
            if (!Uri.TryCreate(settings.Url, UriKind.Absolute, out var url) ||
                url.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(url.UserInfo) ||
                url.Query.Length > 0 || url.Fragment.Length > 0 ||
                !url.AbsolutePath.Equals("/api/qr-menu/pos-bridge", StringComparison.Ordinal))
            {
                reason = "HTTPS QR POS Bridge endpoint is not valid";
                return false;
            }
            return true;
        }

        private async Task<JObject> SendAsync(object command, CancellationToken ct)
        {
            var json = JsonConvert.SerializeObject(command);
            using (var request = new HttpRequestMessage(HttpMethod.Post, settings.Url))
            {
                request.Headers.TryAddWithoutValidation("X-Plugin-Token", settings.PluginIngressToken);
                request.Headers.TryAddWithoutValidation("X-QR-Bridge-Token", settings.BridgeToken);
                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using (var result = await http.SendAsync(request, ct).ConfigureAwait(false))
                {
                    if (result.StatusCode == HttpStatusCode.Redirect ||
                        (int)result.StatusCode >= 300 && (int)result.StatusCode <= 399)
                        throw new InvalidOperationException("QR Bridge redirect was rejected.");
                    var text = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!result.IsSuccessStatusCode)
                    {
                        // Do not log raw provider response or authentication headers.
                        throw new InvalidOperationException("QR Bridge HTTP " + (int)result.StatusCode);
                    }
                    var response = JObject.Parse(text);
                    if (response.Value<bool?>("success") != true)
                        throw new InvalidOperationException("QR Bridge returned an unsuccessful response.");
                    return response;
                }
            }
        }

        private object Command(string action, object extra = null)
        {
            var obj = JObject.FromObject(new {
                action,
                pluginId = identity.PluginId.ToString("D"),
                departmentId = identity.DepartmentId.ToString("D")
            });
            if (extra != null)
                foreach (var property in JObject.FromObject(extra).Properties())
                    obj[property.Name] = property.Value;
            return obj;
        }

        private async Task RunAsync(CancellationToken ct)
        {
            var lastHello = DateTime.MinValue;
            var ready = false;
            var handshakeLogged = false;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (DateTime.UtcNow - lastHello > TimeSpan.FromSeconds(60))
                    {
                        var hello = await SendAsync(Command("hello", new {
                            contractVersion = 1,
                            canCreateOrder = settings.AllowOrderCreation,
                            canAcknowledgeFiscal = settings.AllowOrderCreation,
                            canPrintKitchen = settings.AllowOrderCreation && settings.AllowKitchenPrint
                        }), ct).ConfigureAwait(false);
                        ready = hello.Value<bool?>("accepted") == true;
                        if (ready)
                        {
                            lastHello = DateTime.UtcNow;
                            if (!handshakeLogged)
                            {
                                PluginContext.Log.Info(settings.AllowOrderCreation
                                    ? "QrPosBridge :: HTTPS handshake accepted; test order processing is enabled."
                                    : "QrPosBridge :: HTTPS diagnostic handshake accepted; no POS writes or kitchen printing.");
                                handshakeLogged = true;
                            }
                        }
                    }
                    if (ready && settings.AllowOrderCreation)
                    {
                        await FlushCreatedAcknowledgementsAsync(ct).ConfigureAwait(false);
                        await FlushKitchenAcknowledgementsAsync(ct).ConfigureAwait(false);
                        await FlushFiscalAcknowledgementsAsync(ct).ConfigureAwait(false);
                        var next = await SendAsync(Command("pull"), ct).ConfigureAwait(false);
                        var job = next["job"];
                        if (job is JObject order)
                            await HandleJobAsync(order, ct).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
                catch (Exception e)
                {
                    ready = false;
                    // No secret, guest personal data, or HTTP payload logging.
                    var safeError = e is InvalidOperationException &&
                        e.Message.StartsWith("QR Bridge HTTP ", StringComparison.Ordinal)
                        ? e.Message : e.GetType().Name;
                    PluginContext.Log.Warn("QrPosBridge :: connection paused: " + safeError);
                }
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, Math.Min(60, settings.PollSeconds))), ct)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task HandleJobAsync(JObject job, CancellationToken ct)
        {
            var idText = job.Value<string>("externalOrderId");
            if (!Guid.TryParse(idText, out var id) || id == Guid.Empty)
            {
                PluginContext.Log.Warn("QrPosBridge :: invalid order identifier, refusing to create POS order.");
                return;
            }
            var externalId = id.ToString("D");
            IOrder posOrder = null;
            try
            {
                posOrder = FindExisting(externalId);
                if (posOrder == null)
                {
                    // Journaling BEFORE SubmitChanges closes the crash/timeout duplicate window.
                    if (File.Exists(JournalPath(externalId)))
                    {
                        // A previous SubmitChanges may have committed, but the order is not visible.
                        // Never create a second order without human reconciliation.
                        await AcknowledgeFailureAsync(externalId, "POS_RESULT_UNKNOWN", ct)
                            .ConfigureAwait(false);
                        return;
                    }
                    var planned = Validate(job);
                    StoreJournal(externalId, "PREPARED", null, job.Value<long>("totalMinor"));
                    posOrder = CreatePosOrder(externalId, planned);
                    StoreJournal(externalId, "CREATED", posOrder, job.Value<long>("totalMinor"));
                }
                else if (!File.Exists(JournalPath(externalId)))
                {
                    StoreJournal(externalId, "RECOVERED", posOrder, job.Value<long>("totalMinor"));
                }
            }
            catch (Exception e)
            {
                // If the journal exists, SubmitChanges may have run; don't claim failure.
                // An uncertain operation must be inspected manually and never auto-retried.
                var code = File.Exists(JournalPath(externalId)) ? "POS_RESULT_UNKNOWN" : "POS_VALIDATION_FAILED";
                PluginContext.Log.Warn("QrPosBridge :: order not confirmed: " + e.GetType().Name);
                await AcknowledgeFailureAsync(externalId, code, ct).ConfigureAwait(false);
                return;
            }

            // Durable ack + physical print are resumed from the journal after a restart.
            await FlushCreatedAcknowledgementsAsync(ct).ConfigureAwait(false);
            await FlushKitchenAcknowledgementsAsync(ct).ConfigureAwait(false);
        }

        private static IOrder FindExisting(string externalId)
        {
            var operations = PluginContext.Operations;
            return operations.GetOrders().FirstOrDefault(order =>
                string.Equals(operations.TryGetOrderExternalDataByKey(order, ExternalOrderKey),
                    externalId, StringComparison.OrdinalIgnoreCase));
        }

        private QrCreationPlan Validate(JObject job)
        {
            if (job.Value<string>("currency") != "AZN" ||
                job.Value<string>("paymentMethod") != "PAY_AT_POS" ||
                job.Value<bool?>("onlinePaid") != false ||
                job.Value<string>("rrn") != null)
                throw new InvalidOperationException("Unsupported payment or currency contract.");
            // iikoFront may report AZN as "azn." in terminal configuration.
            // Normalize harmless trailing punctuation, but reject other currencies.
            var posCurrency = (identity.CurrencyCode ?? "").Trim().TrimEnd('.').Trim();
            if (!string.Equals(posCurrency, "AZN", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("POS currency does not match QR currency.");
            var mode = job.Value<string>("mode");
            if (mode != "TABLE" && mode != "FAST_FOOD")
                throw new InvalidOperationException("Unsupported QR service mode.");
            var operations = PluginContext.Operations;
            if (!operations.GetCafeSessions().Any())
                throw new InvalidOperationException("POS cash session is not open.");
            var posTable = job.Value<string>("posTableId");
            ITable table = null;
            if (mode == "TABLE")
            {
                if (!Guid.TryParse(posTable, out var tableId) || tableId == Guid.Empty)
                    throw new InvalidOperationException("QR table has no mapped POS table ID.");
                table = operations.GetTables().FirstOrDefault(x => x.Id == tableId);
                if (table == null)
                    throw new InvalidOperationException("Mapped POS table is not available on this terminal.");
            }
            var items = job["items"] as JArray;
            if (items == null || items.Count == 0 || items.Count > 40)
                throw new InvalidOperationException("Empty or oversized QR basket.");
            var activeProducts = operations.GetActiveProducts().ToDictionary(x => x.Id);
            var planned = new List<QrCreationItem>();
            long totalMinor = 0;
            foreach (var item in items.OfType<JObject>())
            {
                if (!Guid.TryParse(item.Value<string>("productId"), out var productId) ||
                    !activeProducts.TryGetValue(productId, out var product))
                    throw new InvalidOperationException("QR product is not active in POS.");
                var quantity = item.Value<int?>("qty") ?? 0;
                var unitMinor = item.Value<long?>("unitMinor") ?? -1;
                var lineMinor = item.Value<long?>("lineMinor") ?? -1;
                if (quantity < 1 || quantity > 99 || unitMinor <= 0 ||
                    unitMinor > 100000000 || lineMinor != unitMinor * quantity)
                    throw new InvalidOperationException("Invalid QR price or quantity.");
                if (product.Type != ProductType.Dish || product.Template != null || product.Scale != null)
                    throw new InvalidOperationException("QR product requires compound or size configuration.");
                // Until QR modifier mapping is implemented, reject products whose
                // configured modifier groups could create a wrong kitchen recipe.
                if (operations.GetSimpleModifiers(product, null).Any() ||
                    operations.GetGroupModifiers(product, null).Any())
                    throw new InvalidOperationException("QR product modifiers are not yet supported.");
                // NOTE: required modifiers and discounts need separate mapping; disallow mismatches.
                var livePrice = operations.GetPrice(product, null, null, DateTime.Now);
                var liveMinor = decimal.ToInt64(decimal.Round(livePrice * 100m, 0, MidpointRounding.AwayFromZero));
                if (liveMinor != unitMinor)
                    throw new InvalidOperationException("QR price differs from POS price. Republish menu.");
                totalMinor = checked(totalMinor + lineMinor);
                planned.Add(new QrCreationItem { Product = product, Quantity = quantity });
            }
            if (totalMinor != (job.Value<long?>("totalMinor") ?? -1) ||
                totalMinor <= 0 || totalMinor > 50000000)
                throw new InvalidOperationException("QR order total does not match item prices.");
            var guestNote = job.Value<string>("guestNote") ?? "";
            if (guestNote.Length > 280 || guestNote.Any(char.IsControl))
                throw new InvalidOperationException("QR comment contains invalid characters.");
            return new QrCreationPlan { Table = table, Items = planned, Mode = mode,
                GuestNote = guestNote };
        }

        private IOrder CreatePosOrder(string externalId, QrCreationPlan plan)
        {
            var operations = PluginContext.Operations;
            var edit = operations.CreateEditSession();
            var tables = plan.Table == null ? null :
                new[] { plan.Table };
            var newOrder = edit.CreateOrder(tables);
            edit.ChangeOrderOriginName(OriginName, newOrder);
            edit.AddOrderExternalData(ExternalOrderKey, externalId, false, newOrder);
            if (!string.IsNullOrWhiteSpace(plan.GuestNote))
                edit.ChangeOrderComment(plan.GuestNote, newOrder);
            var guest = edit.AddOrderGuest(null, newOrder);
            foreach (var item in plan.Items)
                edit.AddOrderProductItem(item.Quantity, item.Product, newOrder, guest, null);
            var created = operations.SubmitChanges(operations.AuthenticateByPin(settings.OperatorPin), edit);
            return created.Get(newOrder);
        }

        private async Task AcknowledgeFailureAsync(string externalId, string code, CancellationToken ct)
        {
            await SendAsync(Command("ackFailed", new {
                externalOrderId = externalId,
                errorCode = code
            }), ct).ConfigureAwait(false);
        }

        private string JournalPath(string externalId)
        {
            // externalId has already been parsed as Guid; no path traversal.
            return Path.Combine(journalDirectory, externalId + ".json");
        }

        private void StoreJournal(string externalId, string status, IOrder order, long expectedMinor)
        {
            var file = JournalPath(externalId);
            var temp = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            var content = JsonConvert.SerializeObject(new {
                externalOrderId = externalId,
                status,
                posOrderId = order?.Id.ToString("D"),
                posOrderNumber = order?.Number.ToString(CultureInfo.InvariantCulture),
                expectedMinor,
                createdAcked = false,
                kitchenStatus = "NOT_ATTEMPTED",
                kitchenAcked = false,
                createdAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            });
            lock (journalSync)
            {
                File.WriteAllText(temp, content, Encoding.UTF8);
                try
                {
                    if (File.Exists(file)) File.Replace(temp, file, null);
                    else File.Move(temp, file);
                }
                finally { if (File.Exists(temp)) File.Delete(temp); }
            }
        }


        /// <summary>
        /// Called only after a successful physical POS fiscal device operation.
        /// Persist a fiscal acknowledgement before returning from the cheque callback;
        /// network retries are performed by the QR worker, never by the fiscal thread.
        /// A regular order CloseTime alone is NOT proof of fiscalization.
        /// </summary>
        public void CaptureFiscalReceipt(Guid posOrderId, string receiptNumber,
            string fiscalReference, string paymentKind, long paidMinor)
        {
            if (disposed || !settings.Enabled || !settings.AllowOrderCreation ||
                posOrderId == Guid.Empty || string.IsNullOrWhiteSpace(receiptNumber) ||
                string.IsNullOrWhiteSpace(fiscalReference) ||
                (paymentKind != "POS_CASH" && paymentKind != "POS_CARD"))
                return;
            lock (journalSync)
            {
                if (!Directory.Exists(journalDirectory)) return;
                foreach (var file in Directory.EnumerateFiles(journalDirectory, "*.json"))
                {
                    try
                    {
                        var entry = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                        if (!string.Equals(entry.Value<string>("posOrderId"),
                            posOrderId.ToString("D"), StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (entry.Value<long?>("expectedMinor") != paidMinor)
                        {
                            PluginContext.Log.Warn("QrPosBridge :: fiscal amount differs from QR order; reconciliation required.");
                            return;
                        }
                        if (entry.Value<string>("status") == "FISCAL_ACKED") return;
                        if (entry.Value<string>("status") == "FISCAL_PENDING") return;
                        if (entry.Value<string>("status") != "CREATED" &&
                            entry.Value<string>("status") != "RECOVERED")
                            return;
                        entry["status"] = "FISCAL_PENDING";
                        entry["receiptNumber"] = receiptNumber;
                        entry["fiscalReference"] = fiscalReference;
                        entry["paymentKind"] = paymentKind;
                        PersistJournal(file, entry.ToString(Formatting.None));
                        return;
                    }
                    catch (Exception ex)
                    {
                        PluginContext.Log.Warn("QrPosBridge :: fiscal journal unavailable: " +
                            ex.GetType().Name);
                    }
                }
            }
        }


        private async Task FlushCreatedAcknowledgementsAsync(CancellationToken ct)
        {
            if (!Directory.Exists(journalDirectory)) return;
            foreach (var file in Directory.EnumerateFiles(journalDirectory, "*.json"))
            {
                ct.ThrowIfCancellationRequested();
                JObject entry;
                lock (journalSync)
                    entry = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                if (entry.Value<bool?>("createdAcked") == true ||
                    !Guid.TryParse(entry.Value<string>("posOrderId"), out var posOrderId))
                    continue;
                var status = entry.Value<string>("status");
                if (status != "CREATED" && status != "RECOVERED" &&
                    status != "FISCAL_PENDING" && status != "FISCAL_ACKED")
                    continue;
                var live = PluginContext.Operations.GetOrderById(posOrderId);
                if (live == null)
                {
                    PluginContext.Log.Warn("QrPosBridge :: local POS order is missing; manual reconciliation needed.");
                    continue;
                }
                await SendAsync(Command("ackCreated", new {
                    externalOrderId = entry.Value<string>("externalOrderId"),
                    posOrderId = posOrderId.ToString("D"),
                    posOrderNumber = live.Number.ToString(CultureInfo.InvariantCulture)
                }), ct).ConfigureAwait(false);
                lock (journalSync)
                {
                    var latest = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                    latest["createdAcked"] = true;
                    PersistJournal(file, latest.ToString(Formatting.None));
                }
            }
        }

        /// <summary>
        /// Print only after POS creation is acknowledged. A crash or timeout after
        /// marking IN_PROGRESS is uncertain: never print a second time automatically.
        /// The operator must inspect actual kitchen printers and resolve the order.
        /// </summary>
        private void TryPrintKitchen(string file, JObject entry)
        {
            var current = entry.Value<string>("kitchenStatus") ?? "CHECK_REQUIRED";
            if (current == "PRINTED" || current == "CHECK_REQUIRED") return;
            if (current == "IN_PROGRESS")
            {
                SetKitchenState(file, "CHECK_REQUIRED");
                PluginContext.Log.Warn("QrPosBridge :: kitchen print outcome unknown; manual check required.");
                return;
            }
            if (current != "NOT_ATTEMPTED" || entry.Value<bool?>("createdAcked") != true)
                return;
            if (!Guid.TryParse(entry.Value<string>("posOrderId"), out var orderId))
            {
                SetKitchenState(file, "CHECK_REQUIRED");
                return;
            }
            SetKitchenState(file, "IN_PROGRESS");
            try
            {
                var operations = PluginContext.Operations;
                var live = operations.GetOrderById(orderId);
                if (live == null) throw new InvalidOperationException("POS order not found for kitchen print.");
                var items = live.Items.OfType<IOrderCookingItem>().ToList();
                if (items.Count == 0)
                    throw new InvalidOperationException("POS order has no printable cooking items.");
                // This API performs real kitchen service printing; never repeat it
                // after ambiguous failures unless the cashier manually verifies.
                operations.PrintOrderItems(operations.AuthenticateByPin(settings.OperatorPin), live, items);
                SetKitchenState(file, "PRINTED");
            }
            catch (Exception ex)
            {
                SetKitchenState(file, "CHECK_REQUIRED");
                PluginContext.Log.Warn("QrPosBridge :: kitchen print not confirmed: " +
                    ex.GetType().Name);
            }
        }

        private void SetKitchenState(string file, string kitchenStatus)
        {
            lock (journalSync)
            {
                var latest = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                latest["kitchenStatus"] = kitchenStatus;
                latest["kitchenAcked"] = false;
                PersistJournal(file, latest.ToString(Formatting.None));
            }
        }

        private async Task FlushKitchenAcknowledgementsAsync(CancellationToken ct)
        {
            if (!settings.AllowKitchenPrint || !Directory.Exists(journalDirectory)) return;
            foreach (var file in Directory.EnumerateFiles(journalDirectory, "*.json"))
            {
                ct.ThrowIfCancellationRequested();
                JObject entry;
                lock (journalSync)
                    entry = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                if (entry.Value<bool?>("createdAcked") != true) continue;
                // Legacy QR journal records might predate kitchen-dispatch tracking.
                // Their print outcome is unknown: never send them to the printer again.
                if (entry["kitchenStatus"] == null)
                {
                    SetKitchenState(file, "CHECK_REQUIRED");
                    lock (journalSync)
                        entry = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                }
                var kitchenStatus = entry.Value<string>("kitchenStatus") ?? "CHECK_REQUIRED";
                if (kitchenStatus == "NOT_ATTEMPTED" || kitchenStatus == "IN_PROGRESS")
                {
                    TryPrintKitchen(file, entry);
                    lock (journalSync)
                        entry = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                    kitchenStatus = entry.Value<string>("kitchenStatus") ?? "NOT_ATTEMPTED";
                }
                if (entry.Value<bool?>("kitchenAcked") == true ||
                    (kitchenStatus != "PRINTED" && kitchenStatus != "CHECK_REQUIRED"))
                    continue;
                await SendAsync(Command("ackKitchen", new {
                    externalOrderId = entry.Value<string>("externalOrderId"),
                    posOrderId = entry.Value<string>("posOrderId"),
                    kitchenStatus
                }), ct).ConfigureAwait(false);
                lock (journalSync)
                {
                    var latest = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                    if (latest.Value<string>("kitchenStatus") != kitchenStatus) continue;
                    latest["kitchenAcked"] = true;
                    PersistJournal(file, latest.ToString(Formatting.None));
                }
            }
        }

        private async Task FlushFiscalAcknowledgementsAsync(CancellationToken ct)
        {
            if (!Directory.Exists(journalDirectory)) return;
            foreach (var file in Directory.EnumerateFiles(journalDirectory, "*.json"))
            {
                ct.ThrowIfCancellationRequested();
                JObject entry;
                lock (journalSync)
                {
                    entry = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                }
                if (entry.Value<string>("status") != "FISCAL_PENDING") continue;
                await SendAsync(Command("ackFiscalized", new {
                    externalOrderId = entry.Value<string>("externalOrderId"),
                    posOrderId = entry.Value<string>("posOrderId"),
                    receiptNumber = entry.Value<string>("receiptNumber"),
                    fiscalReference = entry.Value<string>("fiscalReference"),
                    paymentKind = entry.Value<string>("paymentKind")
                }), ct).ConfigureAwait(false);
                lock (journalSync)
                {
                    // Do not overwrite a newer event/receipt written by another thread.
                    var latest = JObject.Parse(File.ReadAllText(file, Encoding.UTF8));
                    if (latest.Value<string>("status") != "FISCAL_PENDING") continue;
                    latest["status"] = "FISCAL_ACKED";
                    PersistJournal(file, latest.ToString(Formatting.None));
                }
            }
        }

        private static void PersistJournal(string file, string content)
        {
            var temp = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temp, content, Encoding.UTF8);
            try { File.Replace(temp, file, null); }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            stop.Cancel();
            // Non-blocking disposal: never delay shutting down the POS host.
            http.Dispose();
            stop.Dispose();
        }

        private sealed class QrCreationItem
        {
            public IProduct Product { get; set; }
            public int Quantity { get; set; }
        }
        private sealed class QrCreationPlan
        {
            public ITable Table { get; set; }
            public List<QrCreationItem> Items { get; set; }
            public string Mode { get; set; }
            public string GuestNote { get; set; }
        }
    }
}
