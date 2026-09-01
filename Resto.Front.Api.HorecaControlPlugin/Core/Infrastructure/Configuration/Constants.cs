using System;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration
{
    /// <summary>
    /// Константы для конфигурации
    /// </summary>
    public static class Constants
    {
        // Socket.IO VPS
        // SocketIOClient uses the URL path as the namespace and Options.Path as the
        // Engine.IO transport path. With namespace /plugin-websocket and path
        // /socket.io the final request is:
        // http://68.233.120.197/plugin-websocket/socket.io/
        public const string DefaultSocketUrl = "http://68.233.120.197/plugin-websocket";
        public const string SocketIoPath = "/socket.io";
        public static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(10);

        // Reconnection
        public const int MaxReconnectionAttempts = 10;
        public const int ReconnectionDelayMs = 30000;
        public const int BaseReconnectDelaySeconds = 1;
        public const int MaxReconnectDelaySeconds = 30;
        public const int ConnectionTimeoutSeconds = 15;

        // Database
        public const int HighRiskOperationBatchSize = 50;
        public const int EventBatchSize = 100;
        public const int MessageBatchSize = 100;

        // Timers
        public const double OrderTimeoutMinutes = 1.0;

        // Retry
        public const int MaxDatabaseRetryAttempts = 3;
        public const int DatabaseRetryDelayMs = 100;

        // Timeouts
        public const int EventSendTimeoutSeconds = 10;
        public const int MessageSendTimeoutSeconds = 10;
    }
}
