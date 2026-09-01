using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration;
using SocketIOClient;
using SocketIOClient.Common;
using SocketIOClient.Serializer.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;

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

            // SocketIO 4.x: путь в URL — namespace (/plugin-websocket), engine.io идёт на /socket.io/.
            // Polling + WinHttpHandler: на net472 дефолтный HttpClient (HttpWebRequest) падает с
            // ProtocolViolationException при отправке Auth-тела handshake.
            // AutoUpgrade=false: сервер отдаёт upgrades:[] — WebSocket upgrade недоступен.
            var socketIoOptions = new SocketIOOptions
            {
                ConnectionTimeout = Constants.ConnectionTimeout,
                Transport = TransportProtocol.Polling,
                AutoUpgrade = false,
                Reconnection = false,
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
                },
                Auth = new Dictionary<string, string>
                {
                    { "login", PluginHelpers.IsDeveloperMode ? debugSettings?.DebugUsername : config.Login },
                    { "password", PluginHelpers.IsDeveloperMode ? debugSettings?.DebugPassword : config.Password },
                    { "serverUrl", config.ServerUrl }
                },
            };

            PluginContext.Log.Info($"SocketIOFactory :: Creating client, url={socketUrl}, transport=Polling, timeout={Constants.ConnectionTimeout.TotalSeconds}s, reconnection=false");

            // Те же настройки, что и у ToJson — единый wire-формат с Newtonsoft.
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
                // WinHttpHandler вместо HttpWebRequest — иначе polling+Auth падает на net472.
                services.AddSingleton<HttpClient>(_ => new HttpClient(new WinHttpHandler()));
            });
        }
    }
}
