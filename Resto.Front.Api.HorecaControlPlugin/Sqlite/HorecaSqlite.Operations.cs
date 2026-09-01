using LinqToDB;
using LinqToDB.Data;
using Resto.Front.Api.Data.Brd;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Security;
using Resto.Front.Api.HorecaControlPlugin.Dto;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Notifiers;
using Resto.Front.Api.HorecaControlPlugin.Sql;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Resto.Front.Api.HorecaControlPlugin.Sqlite;

public partial class HorecaSqlite
{
    #region При запуске собираем данные в память

    private ConcurrentDictionary<int, User> userCache = new();
    public Shift Shift { get; private set; }
    private ConcurrentDictionary<Guid, LocalOrder> LocalOrderList = new();

    public ConcurrentBag<HighRiskOperation> HighRiskOperationList { get; set; }

    public void OnStart()
    {
        Log.Info("HorecaSqlite.OnStart starting...");
        GetOpenShift();
        LoadUsers();
        GetAllHighRiskOperations();
        LoadLocalOrders();
        Log.Info("HorecaSqlite.OnStart started.");
    }

    #endregion

    #region Работа с пользователями

    private void LoadUsers()
    {
        foreach (var user in Users)
        {
            userCache.TryAdd(user.Id, user);
        }

        Log.Info("HorecaSqlite.LoadUsers filled.");
    }

    private KeyValuePair<int, User>
        AddUser(Guid userId, string userName)
    {
        var user = new User
        {
            UserId = userId,
            UserName = userName,
        };
        var id = (int)ExecuteDatabaseOperation<long>(() => (long)this.InsertWithIdentity(user));
        userCache.TryAdd(id, user);
        return userCache.Last();
    }


    private KeyValuePair<int, User> GetUser(IUser iuser)
    {
        long userR = 0;
        var userId = iuser?.Id ?? Guid.Empty;
        var userName = iuser?.Name ?? "(empty)";

        var entry = userCache.FirstOrDefault(x => x.Value.UserId == userId);
        var id = entry.Value == null ? -1 : entry.Key;
        if (id == -1)
        {
            entry = AddUser(userId, userName);
        }

        return entry;
    }

    #endregion

    #region Работа со сменами

    /// <summary>
    /// Получение открытой смены
    /// </summary>
    /// <returns></returns>
    public void GetOpenShift()
    {
        Shift = Shifts?
            .LoadWith(s => s.HighRiskOperations)
            .LoadWith(s => s.Orders)
            .LoadWith(s => s.OpenerUser)
            .LoadWith(s => s.CloserUser)
            .OrderByDescending(x => x.OpenTime).FirstOrDefault(x => x.CloseTime == null);

        if (Shift != null)
        {
            Log.Debug(Shift.ToJson());
            Log.Info("HorecaSqlite.Shift filled.");
        }
        else
        {
            Log.Warn("HorecaSqlite.GetOpenShift :: No open shift found.");
        }
    }

    /// <summary>
    /// Открытие смены
    /// </summary>
    /// <param name="user"></param>
    public void OpenShift(DateTime? dt = null, IUser iuser = null)
    {
        if (dt == null)
            dt = DateTime.Now;

        var user = GetUser(iuser);
        // Полная очистка очередей и рисковых операций предыдущей кассовой смены.
        DeleteAllEvents();
        DeleteAllMessages();
        DeleteAllClosedOrders();
        DeleteAllHighriskOperation();
        IncrementCountOrder();
        Shift = new Shift
        {
            OpenTime = dt.Value,
            OpenerUserId = user.Key,
            OpenerUser = user.Value,
        };
        var id = ExecuteDatabaseOperation<int>(() => this.Insert(Shift));
        Shift.Id = id;
    }


    public void CloseShift(IUser iuser)
    {
        var user = GetUser(iuser);
        if (Shift == null)
        {
            Log.Warn("CloseShift :: Shift is null, nothing to close.");
            return;
        }

        Shift.CloseTime = DateTime.Now;
        Shift.CloserUserId = user.Key;
        Shift.CloserUser = user.Value;

        this.Update(Shift);

        // После закрытия кассовой смены рисковые операции предыдущей смены не должны
        // продолжать отображаться в отчётах до открытия новой.
        DeleteAllHighriskOperation();
    }

    #endregion

    #region Работа с рисковыми операциями

    /// <summary>
    /// Возвращает все рисковые операции
    /// </summary>
    /// <returns></returns>
    public void GetAllHighRiskOperations()
    {
        HighRiskOperationList = HighRiskOperations
            .LoadWith(x => x.Shift)
            .LoadWith(x => x.User)
            .OrderBy(x => x.Date)
            .ToConcurrentBag();
        Log.Info("HorecaSqlite.HighRiskOperationList filled.");
    }

    /// <summary>
    /// Удаляет все рисковые операции за предыдущие смены
    /// </summary>
    private void DeleteAllHighriskOperation()
    {
        HighRiskOperations.Delete();
        GetAllHighRiskOperations();
    }


    /// <summary>
    /// Добавить рисковую операцию
    /// </summary>
    /// <param name="user"></param>
    /// <param name="operation"></param>
    public void AddHighRiskOperation(IUser iuser, string operation)
    {
        var user = GetUser(iuser);

        // Проверяем, что смена открыта перед добавлением операции
        if (Shift == null)
        {
            Log.Warn($"AddHighRiskOperation :: Shift is null, cannot add high risk operation for user {user.Value?.UserName ?? "unknown"}");
            return; // Пропускаем добавление операции, если смена не открыта
        }

        HighRiskOperationList.Add(
            new HighRiskOperation
            {
                UserId = user.Key,
                ShiftId = Shift.Id,
                Shift = Shift,
                User = user.Value,
                Action = operation,
                Date = DateTime.Now,
                TerminalsGroupId = PluginHelpers.GroupName.Id
            }
        );
        var highCount = HighRiskOperationList.Where(x => x.Id == 0).ToList();
        if (highCount.Count > 49)
        {
            ExecuteDatabaseOperation<object>(() => this.BulkCopy(highCount));

            Log.Info("HighRiskOperationList :: Save caching data.");
            GetAllHighRiskOperations();
        }
    }

    /// <summary>
    /// Получить рисковую операцию по UserId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public int GetHighRiskOperations(IUser iuser)
    {
        var user = GetUser(iuser);

        // Проверяем, что смена открыта перед подсчетом операций
        if (Shift == null)
        {
            Log.Debug($"GetHighRiskOperations :: Shift is null for user {user.Value?.UserName ?? "unknown"}, returning 0");
            return 0; // Возвращаем 0, если смена не открыта
        }

        var cnt = HighRiskOperationList
            ?.Where(u => u.UserId == user.Key && u.ShiftId == Shift.Id)
            ?.ToList()?.Count ?? 0;


        // var cnt = HighRiskOperations
        //     .LoadWith(x => x.User)
        //     .LoadWith(x => x.Shift)
        //     ?.Where(u =>
        //         u.UserId == user && u.ShiftId == Shift.Id)?.ToList()?.Count() ?? 0;
        return cnt;
    }

    #endregion

    #region Работа с очередью событий

    public void AddEvent(PluginToServerEvent evt) => ExecuteDatabaseOperation<object>(() =>
    {
        this.Insert(new Event
        {
            DateTime = DateTime.Now,
            Uuid = evt.Uuid,
            EventData = evt.ToJson(),
            IsSent = false,
        });
        return null;
    });

    public List<PluginToServerEvent> GetEvents() =>
        this.Events
            .OrderBy(d => d.DateTime)
            .ToList()
            .Select(evt =>
                evt.EventData
                    .FromJson<PluginToServerEvent>())?.ToList() ?? new List<PluginToServerEvent>();


    public void DeleteEvent(Guid uuid)
    {
        ExecuteDatabaseOperation<object>(() =>
        {
            Events.Where(e => e.Uuid == uuid).Delete();
            return null;
        });
    }


    public void DeleteAllSentEvents()
    {
        ExecuteDatabaseOperation<object>(() =>
        {
            Events.Where(e => e.IsSent).Delete();
            return null;
        });
    }

    private void DeleteAllEvents()
    {
        ExecuteDatabaseOperation<object>(() =>
        {
            Events.Delete();
            return null;
        });
    }

    #endregion

    #region Работа с очередью сообщений

    public void AddMessage(PluginEventData msg) => ExecuteDatabaseOperation<object>(() =>
    {
        this.Insert(new Message()
        {
            DateTime = DateTime.Now,
            Uuid = msg.Uuid,
            MessageData = msg.ToJson(),
        });
        return null;
    });

    public List<PluginEventData> GetMessages() =>
        Messages
            .OrderBy(d => d.DateTime)
            .ToList()
            .Select(evt =>
                evt.MessageData
                    .FromJson<PluginEventData>())?.ToList() ?? new List<PluginEventData>();

    public void DeleteMessage(Guid uuid)
    {
        ExecuteDatabaseOperation<object>(() =>
        {
            Messages.Where(e => e.Uuid == uuid).Delete();
            return null;
        });
    }

    public void DeleteAllSentMessages()
    {
        ExecuteDatabaseOperation<object>(() =>
        {
            Messages?.Where(e => e.IsSent).Delete();
            return null;
        });
    }

    private void DeleteAllMessages()
    {
        ExecuteDatabaseOperation<object>(() =>
        {
            Messages.Delete();
            return null;
        });
    }

    #endregion


    #region Работа с заказами

    private void LoadLocalOrders()
    {
        Log.Info("Loading orders from iiko...");


        foreach (var order in PluginContext.Operations.GetOrders(true))
        {
            var deleted = order.StornedOrderId != null
                          || order.Status == OrderStatus.Deleted || order.Status == OrderStatus.Closed
                ;

            var localOrder = GenerateLocalOrders(order, deleted);

            // Проверяем, что смена открыта перед сохранением заказа
            if (Shift == null)
            {
                Log.Warn($"LoadLocalOrders :: Shift is null, skipping order {order.Id}. Cannot save order without open shift.");
                continue;
            }

            ExecuteDatabaseOperation<object>(() =>
            {
                Orders.InsertOrUpdate(
                    () => new Order
                    {
                        OrderId = order.Id,
                        Deleted = localOrder.ToDelete,
                        Data = localOrder.ToJson(),
                        Count = 0,
                        ShiftId = Shift.Id,
                        DateTime = DateTime.Now
                    },
                    x => new Order
                    {
                        Deleted = localOrder.ToDelete,
                        Data = localOrder.ToJson(),
                    },
                    () => new Order { OrderId = order.Id }
                );
                return null;
            });
            LocalOrderList.TryAdd(localOrder.OrderId, localOrder);
        }


        Log.Info("HorecaSqlite.LocalOrderList filled.");
    }

    public void DeleteAllClosedOrders()
    {
        List<Guid> orderIds = new List<Guid>();
        foreach (var ord in LocalOrderList)
        {
            if (ord.Value.ToDelete)
            {
                orderIds.Add(ord.Key);
            }
        }

        if (!orderIds.Any()) return;
        ExecuteDatabaseOperation<object>(() =>
        {
            Orders.Where(x => orderIds.Contains(x.OrderId)).Delete();
            return null;
        });

        foreach (var orderId in orderIds)
            LocalOrderList.TryRemove(orderId, out _);
    }

    //TODO это работает не правильно
    public void IncrementCountOrder()
    {
        if (firstTimeStart)
            return;

        var localOrders = new List<LocalOrder>();
        var dict = new List<OrderData>();
        foreach (var item in LocalOrderList)
        {
            if (!item.Value.ToDelete)
            {
                item.Value.ShiftCount++;
                localOrders.Add(item.Value);
                dict.Add(new OrderData
                {
                    OrderId = item.Key,
                    Data = item.Value.ToJson()
                });
            }
        }

        var orderIds = localOrders.Select(x => x.OrderId).ToList();
        if (orderIds.Count > 0)
            ExecuteDatabaseOperation<object>(() =>
            {
                Orders.Where(x => orderIds.Contains(x.OrderId))
                    .Set(cnt => cnt.Count, cnt => cnt.Count + 1) // Увеличиваем значение на 1
                    .Set(lol => lol.Data, lol =>
                        dict.First(u => u.OrderId == lol.OrderId).Data
                    )
                    .Update();
                return null;
            });
    }

    /// <summary>
    /// Загрузить заказ из локального кэша по Id
    /// </summary>
    public LocalOrder LoadOrder(Guid orderId)
    {
        return LocalOrderList.TryGetValue(orderId, out var localOrder) ? localOrder : null;
    }

    /// <summary>
    /// Загрузить заказ
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public LocalOrder LoadOrder(IOrder order)
    {
        if (order == null)
            return null;
        return LoadOrder(order.Id);
    }

    /// <summary>
    /// Загрузить незакрытые заказы
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public List<LocalOrder> ShiftOpenLoadNonClosingOrders()

    {
        var localOrders = new List<LocalOrder>();
        foreach (var locals in LocalOrderList)
        {
            if (locals.Value.ShiftCount > 0)
                localOrders.Add(locals.Value);
        }

        return localOrders;
    }


    private LocalOrder GenerateLocalOrders(IOrder order, bool deleted)
    {
        var appliedDiscountList = LocalOrder.GetDiscountsSurchargesLists(order);
        bool isBanquet = false;

        DeliveryStatus? deliveryStatus = null;

        var waiter = order.Waiter;

        if (order is IDeliveryOrder deliveryOrder)
        {
            waiter = deliveryOrder.DeliveryOperator;
            deliveryStatus = deliveryOrder.DeliveryStatus;
            if (deliveryStatus is DeliveryStatus.Delivered or DeliveryStatus.Closed)
                deleted = true;
        }

        IReserve tgReserveOrder = null;
        if (order is IOrder)
        {
            tgReserveOrder = PluginContext.Operations.GetReserves()?.FirstOrDefault(x =>
                x.Tables[0].RestaurantSection?.TerminalsGroup?.Id == PluginHelpers.GroupName.Id
                && x.Order != null && x.Order?.Id == order.Id);
        }

        isBanquet = tgReserveOrder != null;


        var localOrder = new LocalOrder
        {
            OrderId = order.Id,
            IsBanquet = order.IsBanquetOrder || isBanquet,
            //ClientName = (order.Reserve != null)? order.Reserve?.Client?.Name: "",
            //Phone = (order.Reserve != null) ? order.Reserve?.Client?.Phones.FirstOrDefault(x=>x.IsMain)?.Value ?? ""  : "",
            Discounts = appliedDiscountList.Discounts,
            Surcharges = appliedDiscountList.Surcharges,
            WaiterName = order.Waiter?.Name ?? string.Empty,
            Number = order.Number,
            Tables = LocalOrder.GetTables(order),
            WaiterId = waiter?.Id ?? Guid.Empty,
            ResultSum = order.ResultSum,
            Floor = order.Tables[0]?.RestaurantSection?.Name ?? string.Empty,
            Revision = order.Revision,
            DeliveryStatus = deliveryStatus,
            LastChangeTerminalId = order.LastChangedTerminalId,
            OpenTime = order.OpenTime,
            CloseTime = order.CloseTime,
            BillTime = order.BillTime,
            ToDelete = deleted,
        };
        return localOrder;
    }

    /// <summary>
    /// Сохранить заказ
    /// </summary>
    /// <param name="order"></param>
    /// <param name="deleted"></param>
    /// <returns></returns>
    public LocalOrder SaveOrder(IOrder order, bool deleted = false)

    {
        bool newOrder = !LocalOrderList.TryGetValue(order.Id, out _);
        var localOrder = GenerateLocalOrders(order, deleted);

        LocalOrderList.AddOrUpdate(order.Id, localOrder, (key, oldValue) => localOrder);


        // Проверяем, что смена открыта перед сохранением заказа
        if (Shift == null)
        {
            Log.Warn($"SaveOrder :: Shift is null for order {order.Id}. Cannot save order without open shift.");
            return localOrder; // Возвращаем localOrder, но не сохраняем в БД
        }

        ExecuteDatabaseOperation<object>(() =>
        {
            var res = 0;
            if (newOrder || deleted)

                res = Orders.InsertOrUpdate(
                    () => new Order
                    {
                        OrderId = order.Id,
                        Data = localOrder.ToJson(),
                        Count = 0,
                        Deleted = localOrder.ToDelete,
                        DateTime = DateTime.Now,
                        ShiftId = Shift.Id,
                    }, // Insert
                    x => new Order
                    {
                        Data = localOrder.ToJson(),
                        Deleted = localOrder.ToDelete,
                    }
                    , () => new Order { OrderId = order.Id }
                );

            // Удаляем из кэша только удалённые заказы.
            // Раньше TryRemove вызывался и после insert нового заказа — из‑за этого
            // каждый следующий Updated снова считался «новым» (дубли newOrder/orderCreated).
            if (deleted && res > 0)
                LocalOrderList.TryRemove(order.Id, out _);

            return null;
        });


        return localOrder;
    }

    #endregion

    #region Private Methods

    private object lockShiftControlDBContext = new();
    // private object lockShiftControlDBContext1 = new();
    //

    private T ExecuteDatabaseOperation<T>(Func<T> func)
    {
        var st = new StackTrace();
        var sf = st.GetFrame(1);
        var methodName = sf.GetMethod().Name;
        T result = default(T);
        int attempts = 0;
        int maxRetries = 3;
        while (attempts < maxRetries)
        {
            lock (lockShiftControlDBContext)
            {
                using (var transaction = BeginTransaction())
                {
                    try
                    {
                        Log.Debug($"Sql.{methodName} :: started.", PluginHelpers.IsDeveloperMode);
                        result = func();
                        transaction.Commit();

                        Log.Debug($"Sql.{methodName} :: finished.", PluginHelpers.IsDeveloperMode);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Log.Error(
                            $"Sql.{methodName} :: Database operation attempt {attempts + 1} failed: {ExceptionMessages(ex)}");
                        if (++attempts >= maxRetries)
                        {
                            Log.Error($"Sql.{methodName} :: All attempts failed.");
                        }

                        Thread.Sleep(100 * attempts);
                    }
                }
            }
        }

        return result;
    }


    private static string ExceptionMessages(Exception ex)
    {
        if (ex.InnerException == null)
            return ex.ToString();
        return ex.ToString() + "  " + ExceptionMessages(ex.InnerException);
    }

    #endregion

    #region Properties

    private new static ILog Log => PluginContext.Log;

    #endregion

    #region Disposable

    public new void Dispose()
    {
        try
        {
            Log.Info("Running disposing...");
            // При закрытии сохраним все 
            var list = HighRiskOperationList.Where(x => x.Id == 0).ToList();
            this.BulkCopy(list);
            Log.Info($"All non-saved HighRiskOperation ({list.Count}) saved.");

            base.Dispose();
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            Log.Error($"Error on disposing: {ex}");
        }
    }

    #endregion
}

public class OrderData
{
    public Guid OrderId { get; set; }
    public string Data { get; set; }
}