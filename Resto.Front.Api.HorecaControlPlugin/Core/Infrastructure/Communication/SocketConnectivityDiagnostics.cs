using System;
using System.Net.Http;
using System.Threading.Tasks;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication
{
    /// <summary>
    /// Диагностика доступности VPS Socket.IO endpoint из процесса iiko.
    /// Не устанавливает Socket.IO-соединение и используется только для диагностики.
    /// </summary>
    public static class SocketConnectivityDiagnostics
    {
        public static void TestHttpEndpoint(string socketUrl)
        {
            if (string.IsNullOrWhiteSpace(socketUrl))
            {
                PluginContext.Log.Error("SocketConnectivityDiagnostics :: Socket URL is empty.");
                return;
            }

            var endpoint = socketUrl.TrimEnd('/') + "/" +
                           Constants.SocketIoPath.Trim('/') +
                           "/?EIO=4&transport=polling";

            PluginContext.Log.Info("==================================================");
            PluginContext.Log.Info("SocketConnectivityDiagnostics :: HTTP TEST START");
            PluginContext.Log.Info($"HTTP endpoint: {endpoint}");

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Constants.ConnectionTimeout;

                    var task = client.GetAsync(endpoint, HttpCompletionOption.ResponseContentRead);
                    task.Wait();

                    var response = task.GetAwaiter().GetResult();
                    var body = response.Content.ReadAsStringAsync()
                        .GetAwaiter()
                        .GetResult();

                    var preview = body ?? string.Empty;
                    if (preview.Length > 500)
                        preview = preview.Substring(0, 500);

                    PluginContext.Log.Info(
                        $"SocketConnectivityDiagnostics :: HTTP status={(int)response.StatusCode} {response.StatusCode}");
                    PluginContext.Log.Info(
                        $"SocketConnectivityDiagnostics :: Response length={body?.Length ?? 0}");
                    PluginContext.Log.Info(
                        $"SocketConnectivityDiagnostics :: Response preview={preview}");
                    PluginContext.Log.Info("SocketConnectivityDiagnostics :: HTTP TEST SUCCESS");
                }
            }
            catch (AggregateException ex)
            {
                var root = ex.GetBaseException();
                PluginContext.Log.Error(
                    $"SocketConnectivityDiagnostics :: HTTP TEST FAILED: {root.GetType().FullName}: {root.Message}",
                    root);

                if (root.InnerException != null)
                {
                    PluginContext.Log.Error(
                        $"SocketConnectivityDiagnostics :: InnerException: {root.InnerException.GetType().FullName}: {root.InnerException.Message}",
                        root.InnerException);
                }
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(
                    $"SocketConnectivityDiagnostics :: HTTP TEST FAILED: {ex.GetType().FullName}: {ex.Message}",
                    ex);

                if (ex.InnerException != null)
                {
                    PluginContext.Log.Error(
                        $"SocketConnectivityDiagnostics :: InnerException: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}",
                        ex.InnerException);
                }
            }

            PluginContext.Log.Info("SocketConnectivityDiagnostics :: HTTP TEST END");
            PluginContext.Log.Info("==================================================");
        }
    }
}
