using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration;
using SocketIOClient;
using SocketIOClient.Common;
using SocketIOClient.Serializer.NewtonsoftJson;
using System;
using System.Collections.Specialized;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication
{
    /// <summary>
    /// Фабрика для создания SocketIO клиента
    /// </summary>
    public static class SocketIOFactory
    {
        /// <summary>
        /// Создает и настраивает SocketIO клиент
        /// </summary>
        public static SocketIOClient.SocketIO CreateClient(SocketIoConnectorConfig config, DebugSettings debugSettings)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var socketUrl = PluginHelpers.IsDeveloperMode && debugSettings?.DebugSocketUrl != null
                ? debugSettings.DebugSocketUrl
                : Constants.DefaultSocketUrl;

            // VPS Socket.IO:
            // Namespace: /plugin-websocket
            // Engine.IO path: /plugin-websocket/socket.io
            // Transport: polling only
            // Authentication: disabled
            // Plugin identity is sent through Query.
            var socketIoOptions = new SocketIOOptions
            {
                ConnectionTimeout = Constants.ConnectionTimeout,
                Transport = TransportProtocol.Polling,
                AutoUpgrade = false,
                Reconnection = false,
                Path = Constants.SocketIoPath,
                Query = new NameValueCollection
                {
                    ["pluginId"] = config.PluginId.ToString(),
                    ["pluginName"] = config.PluginName ?? string.Empty,
                    ["groupId"] = config.GroupId.ToString(),
                    ["groupName"] = config.GroupName ?? string.Empty,
                    ["departmentId"] = config.DepartmentId.ToString(),
                    ["departmentName"] = config.DepartmentName ?? string.Empty,
                    ["version"] = config.Version ?? string.Empty,
                    ["currencyCode"] = config.CurrencyCode ?? string.Empty,
                }
            };

            PluginContext.Log.Info(
                $"SocketIOFactory :: Creating client, url={socketUrl}, path={Constants.SocketIoPath}, namespace=/plugin-websocket, transport=Polling, auth=disabled, timeout={Constants.ConnectionTimeout.TotalSeconds}s, reconnection=false");

            var socketJsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = PluginHelpers.jsonSerializerSettings.NullValueHandling,
                Formatting = PluginHelpers.jsonSerializerSettings.Formatting,
                ReferenceLoopHandling = PluginHelpers.jsonSerializerSettings.ReferenceLoopHandling,
                PreserveReferencesHandling = PluginHelpers.jsonSerializerSettings.PreserveReferencesHandling,
                Converters = PluginHelpers.jsonSerializerSettings.Converters,
            };

            return new SocketIOClient.SocketIO(new Uri(socketUrl), socketIoOptions, services =>
            {
                services.AddNewtonsoftJson(socketJsonSettings);
            });
        }
    }
}
