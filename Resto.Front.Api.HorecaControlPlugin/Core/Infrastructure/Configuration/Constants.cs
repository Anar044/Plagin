using System;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration
{
    /// <summary>
    /// Constants for plugin configuration.
    /// </summary>
    public static class Constants
    {
        // Socket.IO VPS
        // SocketIOClient uses the URI path as the Socket.IO namespace,
        // while Options.Path is the Engine.IO transport endpoint.
        //
        // Therefore the correct combination is:
        //   URI  = http://68.233.120.197/plugin-websocket
        //   Path = /socket.io
        //
        // This produces the Engine.IO endpoint:
        //   http://68.233.120.197/plugin-websocket/socket.io/
        //
        // IMPORTANT: do not put /plugin-websocket into both values,
        // otherwise the client requests /plugin-websocket/plugin-websocket/socket.io.
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
