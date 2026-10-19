using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration;
using SocketIOClient;
using SocketIOClient.Common;
using SocketIOClient.Serializer.NewtonsoftJson;
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication
{
    /// <summary>
    /// Factory for creating the Socket.IO client.
    /// </summary>
    public static class SocketIOFactory
    {
        private sealed class AuthenticationBinding
        {
            internal NameValueCollection Query;
            internal SocketIoConnectorConfig Config;
            internal void Refresh() => SocketHandshakeAuth.Refresh(Query, Config);
        }

        private static readonly ConditionalWeakTable<SocketIOClient.SocketIO, AuthenticationBinding>
            AuthenticationBindings = new ConditionalWeakTable<SocketIOClient.SocketIO, AuthenticationBinding>();

        // Called before each ConnectAsync, including retries on the SAME client.
        public static void RefreshAuthentication(SocketIOClient.SocketIO client)
        {
            if (client != null && AuthenticationBindings.TryGetValue(client, out var binding))
                binding.Refresh();
        }
        public static SocketIOClient.SocketIO CreateClient(SocketIoConnectorConfig config, DebugSettings debugSettings)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // IMPORTANT:
            // SocketIOClient combines the base URL with Options.Path.
            // Therefore the base URL must NOT already contain /plugin-websocket.
            // VPS layout:
            //   base URL  = http://68.233.120.197
            //   namespace = /plugin-websocket
            //   path      = /plugin-websocket/socket.io
            var socketUrl = PluginHelpers.IsDeveloperMode && !string.IsNullOrWhiteSpace(debugSettings?.DebugSocketUrl)
                ? debugSettings.DebugSocketUrl
                : Constants.DefaultSocketUrl;

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
                $"SocketIOFactory :: Creating client, url={socketUrl}, path={Constants.SocketIoPath}, namespace=/plugin-websocket, transport=Polling, auth={(string.IsNullOrWhiteSpace(config.SocketAuthSecret) ? "legacy" : "hmac-sha256-v1")}, timeout={Constants.ConnectionTimeout.TotalSeconds}s, reconnection=false");

            var socketJsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = PluginHelpers.jsonSerializerSettings.NullValueHandling,
                Formatting = PluginHelpers.jsonSerializerSettings.Formatting,
                ReferenceLoopHandling = PluginHelpers.jsonSerializerSettings.ReferenceLoopHandling,
                PreserveReferencesHandling = PluginHelpers.jsonSerializerSettings.PreserveReferencesHandling,
                Converters = PluginHelpers.jsonSerializerSettings.Converters,
            };

            var client = new SocketIOClient.SocketIO(new Uri(socketUrl), socketIoOptions, services =>
            {
                services.AddNewtonsoftJson(socketJsonSettings);

                // SocketIOClient expects HttpClient from DI.
                // WinHttpHandler is a HttpMessageHandler, so wrap it in HttpClient.
                // This is required for stable HTTP polling on .NET Framework 4.7.2.
                services.AddSingleton<HttpClient>(_ =>
                {
                    var handler = new System.Net.Http.WinHttpHandler();
                    return new HttpClient(handler);
                });
            });
            if (!string.IsNullOrWhiteSpace(config.SocketAuthSecret))
            {
                var binding = new AuthenticationBinding
                {
                    Query = socketIoOptions.Query,
                    Config = config
                };
                binding.Refresh();
                AuthenticationBindings.Add(client, binding);
            }
            return client;
        }
    }
}
