using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Dto;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using Resto.Front.Api.HorecaControlPlugin.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication
{
    /// <summary>
    /// Управление WebSocket соединением (синхронный)
    /// </summary>
    public class SocketConnectionManager : ISocketConnectionManager
    {
        private readonly SocketIOClient.SocketIO _client;
        private readonly IRepository _repository;
        private readonly SemaphoreSlim _reconnectSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(1, 1);
        private volatile int _alreadyConnected = 0; // 0 = false, 1 = true
        private volatile int _reconnectAttempt;
        private volatile int _disposed; // 0 = false, 1 = true
        private volatile int _hasEverConnected; // 0 = false, 1 = true

        public bool IsConnected => _client?.Connected ?? false;

        public event EventHandler Connected;
        public event EventHandler<string> Disconnected;
        public event EventHandler<string> Error;
        public event EventHandler<int> Reconnected;

        public SocketConnectionManager(SocketIOClient.SocketIO client, IRepository repository)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _client.OnConnected += (sender, args) =>
            {
                PluginContext.Log.Info("SocketConnectionManager :: Client connected.");

                Interlocked.Exchange(ref _alreadyConnected, 1);

                var attempt = _reconnectAttempt;
                _reconnectAttempt = 0;

                // Reconnected только после реального разрыва; OnReconnectAttempt(1) при первом connect игнорируем.
                if (Interlocked.CompareExchange(ref _hasEverConnected, 1, 0) == 0)
                {
                    Connected?.Invoke(this, EventArgs.Empty);
                }
                else if (attempt > 0)
                {
                    Reconnected?.Invoke(this, attempt);
                }
                else
                {
                    Connected?.Invoke(this, EventArgs.Empty);
                }
            };

            _client.OnDisconnected += (sender, reason) =>
            {
                try
                {
                    PluginContext.Log.Info($"SocketConnectionManager :: Client disconnected. Reason: {reason}");
                    Interlocked.Exchange(ref _alreadyConnected, 0);

                    Disconnected?.Invoke(this, reason);

                    if (_disposed != 0)
                        return;

                    // Запускаем реконнект в фоне
                    Task.Run(() => AttemptReconnectAsync().ConfigureAwait(false));
                }
                catch (Exception ex)
                {
                    Error?.Invoke(this, $"OnDisconnected error: {ex.Message}");
                    PluginContext.Log.Error($"OnDisconnected handler error: {ex.Message}", ex);
                }
            };

            _client.OnError += (sender, error) =>
            {
                PluginContext.Log.Error($"SocketConnectionManager :: Client error: {error}");
                // Логируем детали URL из ошибки для диагностики
                if (!string.IsNullOrEmpty(error))
                {
                    try
                    {
                        var ex = new Exception(error);
                        LogWebSocketUrlFromError(ex);
                    }
                    catch
                    {
                        // Игнорируем ошибки при логировании
                    }
                }
                Error?.Invoke(this, error);
            };

            _client.OnReconnectAttempt += (sender, attempt) =>
            {
                _reconnectAttempt = attempt;
                PluginContext.Log.Info($"SocketConnectionManager :: Reconnect attempt {attempt}.");
            };
        }

        public void Connect(int maxRetries = 5)
        {
            if (_disposed != 0)
            {
                PluginContext.Log.Warn("Connect :: Already disposed, skipping...");
                return;
            }

            // Защита от одновременных вызовов Connect
            if (!_connectSemaphore.Wait(0))
            {
                PluginContext.Log.Info("Connect :: Connection attempt already in progress, skipping...");
                return;
            }

            try
            {
                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    if (_disposed != 0)
                    {
                        PluginContext.Log.Info("Connect :: Dispose requested, aborting connection attempts.");
                        return;
                    }

                    try
                    {
                        if (IsConnected)
                        {
                            PluginContext.Log.Info($"Connect :: Already connected, skipping attempt {attempt + 1}");
                            return;
                        }

                        PluginContext.Log.Info($"Connect :: Attempt {attempt + 1}/{maxRetries}");
                        PluginContext.Log.Info($"Connect :: Current connection status - IsConnected: {IsConnected}, Client.Connected: {_client?.Connected ?? false}");

                        // Синхронное ожидание подключения с таймаутом
                        PluginContext.Log.Debug($"Connect :: Starting ConnectAsync with timeout {Constants.ConnectionTimeoutSeconds}s");
                        var connectTask = _client.ConnectAsync();
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(Constants.ConnectionTimeoutSeconds));
                        var completedTask = Task.WaitAny(connectTask, timeoutTask);

                        if (completedTask == 0)
                        {
                            // ConnectAsync завершился
                            try
                            {
                                SynchronousTaskHelper.WaitForCompletion(connectTask);

                                if (IsConnected)
                                {
                                    PluginContext.Log.Info($"Connect :: Successfully connected on attempt {attempt + 1}");
                                    return;
                                }
                                else
                                {
                                    PluginContext.Log.Warn($"Connect :: Attempt {attempt + 1} completed but connection status is false. Connection may have failed.");
                                    // Проверяем, есть ли исключение в задаче
                                    if (connectTask.IsFaulted && connectTask.Exception != null)
                                    {
                                        var ex = connectTask.Exception.GetBaseException();
                                        PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} task faulted: {ex.Message}", ex);
                                        LogWebSocketUrlFromError(ex);
                                    }
                                }
                            }
                            catch (Exception taskEx)
                            {
                                PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} task completion error: {taskEx.Message}", taskEx);
                                LogWebSocketUrlFromError(taskEx);
                            }
                        }
                        else
                        {
                            // Таймаут - проверяем состояние задачи подключения
                            PluginContext.Log.Warn($"Connect :: Attempt {attempt + 1} timed out after {Constants.ConnectionTimeoutSeconds} seconds.");

                            // Пытаемся получить информацию об ошибке, если задача завершилась с ошибкой
                            if (connectTask.IsCompleted)
                            {
                                if (connectTask.IsFaulted && connectTask.Exception != null)
                                {
                                    var ex = connectTask.Exception.GetBaseException();
                                    PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} connection task failed: {ex.Message}", ex);
                                    LogWebSocketUrlFromError(ex);
                                }
                                else if (connectTask.IsCanceled)
                                {
                                    PluginContext.Log.Warn($"Connect :: Attempt {attempt + 1} connection task was canceled.");
                                }
                            }
                            else
                            {
                                PluginContext.Log.Warn($"Connect :: Attempt {attempt + 1} connection task is still running after timeout. Task status: {connectTask.Status}");
                            }

                        // Создаем исключение для логирования деталей таймаута
                        PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} - Creating timeout exception for detailed logging");
                        var timeoutEx = new TimeoutException($"Connection attempt {attempt + 1} timed out after {Constants.ConnectionTimeoutSeconds} seconds. IsConnected: {IsConnected}, TaskStatus: {connectTask.Status}");
                        PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} - Calling LogWebSocketUrlFromError for timeout");
                        LogWebSocketUrlFromError(timeoutEx);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        PluginContext.Log.Warn($"Connect :: Attempt {attempt + 1} aborted (plugin domain unloading).");
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (_disposed != 0)
                        {
                            PluginContext.Log.Info("Connect :: Dispose requested during error handling, stopping.");
                            return;
                        }

                        PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} failed: {ex.Message}", ex);
                        PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} exception details: {ex.GetType().Name}, StackTrace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                            PluginContext.Log.Error($"Connect :: Attempt {attempt + 1} inner exception StackTrace: {ex.InnerException.StackTrace}");
                        }
                        LogWebSocketUrlFromError(ex);
                    }

                    // Экспоненциальная задержка перед следующей попыткой
                    if (attempt < maxRetries - 1 && _disposed == 0)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(Constants.BaseReconnectDelaySeconds, attempt));
                        PluginContext.Log.Info($"Connect :: Waiting {delay.TotalSeconds}s before next attempt");
                        Thread.Sleep(delay);
                    }
                }

                if (!IsConnected && _disposed == 0)
                {
                    PluginContext.Log.Error($"Connect :: All {maxRetries} connection attempts failed. IsConnected: {IsConnected}, Client.Connected: {_client?.Connected ?? false}");
                    PluginContext.Log.Error($"Connect :: Final connection state - Task may have completed but connection was not established.");

                    PluginContext.Log.Error($"Connect :: All attempts failed - Calling LogWebSocketUrlFromError");
                    var finalFailureEx = new Exception($"All {maxRetries} connection attempts failed. Final state - IsConnected: {IsConnected}, Client.Connected: {_client?.Connected ?? false}");
                    LogWebSocketUrlFromError(finalFailureEx);

                    PluginContext.Log.Error($"Connect :: Will retry via AttemptReconnectAsync.");
                    Task.Run(() => AttemptReconnectAsync().ConfigureAwait(false));
                }
            }
            finally
            {
                try
                {
                    _connectSemaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                    // ignore during unload
                }
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            try
            {
                SynchronousTaskHelper.WaitForCompletion(
                    _client.DisconnectAsync(),
                    TimeSpan.FromSeconds(Constants.ConnectionTimeoutSeconds));
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"Disconnect error: {ex.Message}", ex);
            }
        }

        public bool SendEvent(PluginToServerEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            try
            {
                if (!IsConnected)
                {
                    PluginContext.Log.Warn("SendEvent :: Not connected, adding to queue");
                    try
                    {
                        _repository.AddEvent(evt);
                    }
                    catch (ObjectDisposedException)
                    {
                        PluginContext.Log.Warn("SendEvent :: Cannot add event to repository, database is disposed");
                    }
                    return false;
                }

                PluginContext.Log.Debug($"SendEvent ::\n{evt.ToJson()}");

                // Синхронное ожидание отправки с таймаутом
                var task = _client.EmitAsync("plugin_to_server_event", new object[] { evt });
                SynchronousTaskHelper.WaitForCompletion(task, TimeSpan.FromSeconds(Constants.EventSendTimeoutSeconds));

                // В .NET Framework 4.7.2 нет IsCompletedSuccessfully, используем проверку через Status
                if (task.Status == TaskStatus.RanToCompletion && !task.IsFaulted && !task.IsCanceled)
                {
                    PluginContext.Log.Info("SendEvent :: Event sent successfully.");
                    return true;
                }
            }
            catch (TimeoutException ex)
            {
                PluginContext.Log.Warn($"SendEvent :: Timeout: {ex.Message}");
                try
                {
                    _repository.AddEvent(evt);
                }
                catch (ObjectDisposedException)
                {
                    PluginContext.Log.Warn("SendEvent :: Cannot add event to repository, database is disposed");
                }
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                PluginContext.Log.Warn($"SendEvent :: Database is disposed, cannot send or queue event: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"SendEvent :: Error: {ex.Message}", ex);
                try
                {
                    _repository.AddEvent(evt);
                }
                catch (ObjectDisposedException)
                {
                    PluginContext.Log.Warn("SendEvent :: Cannot add event to repository, database is disposed");
                }
                return false;
            }

            return false;
        }

        public bool SendMessage(PluginEventData message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                if (!IsConnected)
                {
                    PluginContext.Log.Warn("SendMessage :: Not connected, adding to queue");
                    _repository.AddMessage(message);
                    return false;
                }

                PluginContext.Log.Debug($"SendMessage ::\n{message.ToJson()}");

                // Синхронное ожидание отправки с таймаутом
                var task = _client.EmitAsync("plugin_to_server", new object[] { message });
                SynchronousTaskHelper.WaitForCompletion(task, TimeSpan.FromSeconds(Constants.MessageSendTimeoutSeconds));

                // В .NET Framework 4.7.2 нет IsCompletedSuccessfully, используем проверку через Status
                if (task.Status == TaskStatus.RanToCompletion && !task.IsFaulted && !task.IsCanceled)
                {
                    PluginContext.Log.Info("SendMessage :: Message sent successfully.");
                    return true;
                }
            }
            catch (TimeoutException ex)
            {
                PluginContext.Log.Warn($"SendMessage :: Timeout: {ex.Message}");
                _repository.AddMessage(message);
                return false;
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"SendMessage :: Error: {ex.Message}", ex);
                _repository.AddMessage(message);
                return false;
            }

            return false;
        }

        public void SendUnsentEvents()
        {
            var unsentEvents = _repository.GetUnsentEvents(Constants.EventBatchSize);

            foreach (var evt in unsentEvents)
            {
                if (SendEvent(evt))
                {
                    _repository.DeleteEvent(evt.Uuid);
                }
            }
        }

        public void SendUnsentMessages()
        {
            var unsentMessages = _repository.GetUnsentMessages(Constants.MessageBatchSize);

            foreach (var message in unsentMessages)
            {
                if (SendMessage(message))
                {
                    _repository.DeleteMessage(message.Uuid);
                }
            }
        }

        private async Task AttemptReconnectAsync()
        {
            if (_disposed != 0)
                return;

            if (!await _reconnectSemaphore.WaitAsync(0).ConfigureAwait(false))
            {
                PluginContext.Log.Info("AttemptReconnectAsync :: Reconnection already in progress, skipping...");
                return;
            }

            try
            {
                if (_disposed != 0)
                    return;

                if (IsConnected)
                    return;

                PluginContext.Log.Info("AttemptReconnectAsync :: Attempting to reconnect...");
                PluginContext.Log.Info($"AttemptReconnectAsync :: Current connection status - IsConnected: {IsConnected}, Client.Connected: {_client?.Connected ?? false}");
                
                await Task.Delay(Constants.ReconnectionDelayMs).ConfigureAwait(false);

                if (_disposed != 0)
                    return;

                try
                {
                    Connect();
                }
                catch (Exception connectEx)
                {
                    PluginContext.Log.Error($"AttemptReconnectAsync :: Connect() threw exception: {connectEx.Message}", connectEx);
                    PluginContext.Log.Error($"AttemptReconnectAsync :: Calling LogWebSocketUrlFromError for Connect() exception");
                    LogWebSocketUrlFromError(connectEx);
                }

                if (IsConnected)
                {
                    PluginContext.Log.Info("AttemptReconnectAsync :: Reconnected successfully.");
                }
                else if (_disposed == 0)
                {
                    PluginContext.Log.Error($"AttemptReconnectAsync :: Reconnection attempt failed. IsConnected: {IsConnected}, Client.Connected: {_client?.Connected ?? false}");
                    PluginContext.Log.Error($"AttemptReconnectAsync :: Connection failed but no exception was thrown. This may indicate a timeout or silent failure.");

                    PluginContext.Log.Error($"AttemptReconnectAsync :: Reconnection failed - Calling LogWebSocketUrlFromError");
                    var reconnectFailureEx = new Exception($"Reconnection failed after Connect() call. IsConnected: {IsConnected}, Client.Connected: {_client?.Connected ?? false}");
                    LogWebSocketUrlFromError(reconnectFailureEx);

                    PluginContext.Log.Warn($"AttemptReconnectAsync :: Will retry after {Constants.ReconnectionDelayMs}ms.");
                    await Task.Delay(Constants.ReconnectionDelayMs).ConfigureAwait(false);
                    if (_disposed == 0)
                        _ = Task.Run(() => AttemptReconnectAsync().ConfigureAwait(false));
                }
            }
            catch (Exception ex)
            {
                if (_disposed != 0)
                    return;

                PluginContext.Log.Error($"AttemptReconnectAsync :: Error: {ex.Message}", ex);
                PluginContext.Log.Error($"AttemptReconnectAsync :: Exception details: {ex.GetType().Name}, StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    PluginContext.Log.Error($"AttemptReconnectAsync :: Inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                    PluginContext.Log.Error($"AttemptReconnectAsync :: Inner exception StackTrace: {ex.InnerException.StackTrace}");
                }
                LogWebSocketUrlFromError(ex);
                await Task.Delay(Constants.ReconnectionDelayMs).ConfigureAwait(false);
                if (_disposed == 0)
                    _ = Task.Run(() => AttemptReconnectAsync().ConfigureAwait(false));
            }
            finally
            {
                try
                {
                    _reconnectSemaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                    // ignore during unload
                }
            }
        }

        /// <summary>
        /// Логирует детали URL из ошибки подключения для диагностики
        /// </summary>
        private void LogWebSocketUrlFromError(Exception e)
        {
            try
            {
                PluginContext.Log.Error("==========================================");
                PluginContext.Log.Error("SocketConnectionManager :: LogWebSocketUrlFromError CALLED");
                PluginContext.Log.Error("==========================================");
                PluginContext.Log.Error($"SocketConnectionManager :: Connection error details:");
                PluginContext.Log.Error($"  - Exception type: {e?.GetType().Name ?? "null"}");
                PluginContext.Log.Error($"  - Error message: {e?.Message ?? "null"}");
                PluginContext.Log.Error($"  - IsConnected: {IsConnected}");
                PluginContext.Log.Error($"  - Client.Connected: {_client?.Connected ?? false}");

                if (e?.InnerException != null)
                {
                    PluginContext.Log.Error($"  - Inner exception type: {e.InnerException.GetType().Name}");
                    PluginContext.Log.Error($"  - Inner exception message: {e.InnerException.Message}");
                    if (e.InnerException.InnerException != null)
                    {
                        PluginContext.Log.Error($"  - Inner-Inner exception: {e.InnerException.InnerException.GetType().Name} - {e.InnerException.InnerException.Message}");
                    }
                }

                string errorMessage = e?.Message ?? e?.ToString() ?? string.Empty;
                string fullErrorText = errorMessage;

                // Собираем все сообщения об ошибках
                if (e?.InnerException != null)
                {
                    fullErrorText += " | " + (e.InnerException.Message ?? e.InnerException.ToString() ?? string.Empty);
                    if (e.InnerException.InnerException != null)
                    {
                        fullErrorText += " | " + (e.InnerException.InnerException.Message ?? e.InnerException.InnerException.ToString() ?? string.Empty);
                    }
                }

                // Ищем URL в различных форматах
                string foundUrl = null;

                // Паттерн 1: 'url'
                var urlMatch1 = Regex.Match(fullErrorText, @"'([^']+)'");
                if (urlMatch1.Success)
                {
                    foundUrl = urlMatch1.Groups[1].Value;
                }

                // Паттерн 2: "url"
                if (string.IsNullOrEmpty(foundUrl))
                {
                    var urlMatch2 = Regex.Match(fullErrorText, @"""([^""]+)""");
                    if (urlMatch2.Success)
                    {
                        foundUrl = urlMatch2.Groups[1].Value;
                    }
                }

                // Паттерн 3: http:// или ws:// в тексте
                if (string.IsNullOrEmpty(foundUrl))
                {
                    var urlMatch3 = Regex.Match(fullErrorText, @"(https?://[^\s'""]+|ws://[^\s'""]+|wss://[^\s'""]+)");
                    if (urlMatch3.Success)
                    {
                        foundUrl = urlMatch3.Groups[1].Value;
                    }
                }

                if (!string.IsNullOrEmpty(foundUrl))
                {
                    PluginContext.Log.Error($"SocketConnectionManager :: Found WebSocket URL in error: {foundUrl}");

                    try
                    {
                        var uri = new Uri(foundUrl);
                        var queryString = uri.Query.TrimStart('?');

                        PluginContext.Log.Error($"SocketConnectionManager :: URL analysis:");
                        PluginContext.Log.Error($"  - Scheme: {uri.Scheme}");
                        PluginContext.Log.Error($"  - Host: {uri.Host}");
                        PluginContext.Log.Error($"  - Port: {uri.Port}");
                        PluginContext.Log.Error($"  - Path: {uri.AbsolutePath}");
                        PluginContext.Log.Error($"  - Query string: {queryString}");

                        if (!string.IsNullOrEmpty(queryString))
                        {
                            PluginContext.Log.Error($"SocketConnectionManager :: Query parameters analysis:");

                            // Парсим query string вручную
                            var parameters = queryString.Split('&');
                            foreach (var param in parameters)
                            {
                                var parts = param.Split(new[] { '=' }, 2);
                                if (parts.Length == 2)
                                {
                                    var key = Uri.UnescapeDataString(parts[0]);
                                    var value = parts[1];

                                    // Проверяем, закодировано ли значение
                                    var decodedValue = Uri.UnescapeDataString(value);
                                    var isEncoded = value != decodedValue ||
                                                   value.Contains("%") ||
                                                   (decodedValue.Any(c => c > 127)); // Проверка на не-ASCII

                                    PluginContext.Log.Error($"  - {key} = '{decodedValue}' (Raw: '{value}', Encoded: {isEncoded})");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        PluginContext.Log.Error($"SocketConnectionManager :: Failed to parse URL '{foundUrl}': {ex.Message}");
                    }
                }
                else
                {
                    PluginContext.Log.Error($"SocketConnectionManager :: Could not extract URL from error message. Full error text: {fullErrorText}");
                }

                // Логируем полный stack trace для диагностики
                if (e?.StackTrace != null)
                {
                    PluginContext.Log.Error($"SocketConnectionManager :: Stack trace: {e.StackTrace}");
                }
                else
                {
                    PluginContext.Log.Error($"SocketConnectionManager :: Stack trace is null");
                }
                
                PluginContext.Log.Error("==========================================");
                PluginContext.Log.Error("SocketConnectionManager :: LogWebSocketUrlFromError COMPLETED");
                PluginContext.Log.Error("==========================================");
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error($"SocketConnectionManager :: Error in LogWebSocketUrlFromError: {ex.Message}", ex);
                PluginContext.Log.Error($"SocketConnectionManager :: LogWebSocketUrlFromError inner error StackTrace: {ex.StackTrace}");
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            try
            {
                // Всегда пытаемся разорвать сессию — в т.ч. пока ConnectAsync ещё идёт
                SynchronousTaskHelper.WaitForCompletion(
                    _client.DisconnectAsync(),
                    TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                PluginContext.Log.Warn($"Dispose :: DisconnectAsync: {ex.Message}");
            }

            try
            {
                _client?.Dispose();
            }
            catch (Exception ex)
            {
                PluginContext.Log.Warn($"Dispose :: Client.Dispose: {ex.Message}");
            }

            try { _reconnectSemaphore?.Dispose(); } catch { /* ignore */ }
            try { _connectSemaphore?.Dispose(); } catch { /* ignore */ }
        }
    }
}

