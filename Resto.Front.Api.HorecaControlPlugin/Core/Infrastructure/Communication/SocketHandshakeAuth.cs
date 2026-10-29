using Resto.Front.Api.HorecaControlPlugin;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication
{
    /// <summary>
    /// Optional Socket.IO HMAC-SHA256-v1 handshake used by Smart Horeca VPS.
    /// A new timestamp and cryptographic nonce MUST be generated for each
    /// ConnectAsync attempt to avoid replay and expired-auth failures.
    /// The secret itself is never sent or logged.
    /// </summary>
    internal static class SocketHandshakeAuth
    {
        internal static void Refresh(NameValueCollection query, SocketIoConnectorConfig config)
        {
            if (query == null || config == null)
                return;

            var secret = config.SocketAuthSecret;
            if (string.IsNullOrWhiteSpace(secret))
            {
                // Default behaviour for existing installations is unchanged.
                query.Remove("authMode");
                query.Remove("authTs");
                query.Remove("authNonce");
                query.Remove("authSig");
                return;
            }

            // Identity and casing must match the existing Socket.IO query.
            var pluginId = config.PluginId.ToString();
            var departmentId = config.DepartmentId.ToString();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                .ToString(CultureInfo.InvariantCulture);

            var bytes = new byte[24];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            var nonce = Convert.ToBase64String(bytes).TrimEnd('=')
                .Replace('+', '-').Replace('/', '_');
            var payload = pluginId + "|" + departmentId + "|" + timestamp + "|" + nonce;
            byte[] signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret.Trim())))
                signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

            var hex = new StringBuilder(signature.Length * 2);
            foreach (var b in signature)
                hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));

            query["authMode"] = "hmac-sha256-v1";
            query["authTs"] = timestamp;
            query["authNonce"] = nonce;
            query["authSig"] = hex.ToString();
        }
    }
}
