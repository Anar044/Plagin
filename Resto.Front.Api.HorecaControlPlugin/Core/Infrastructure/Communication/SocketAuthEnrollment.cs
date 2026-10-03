using Newtonsoft.Json;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration;
using System;
using System.Net.Http;
using System.Text;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication
{
    internal static class SocketAuthEnrollment
    {
        private sealed class EnrollmentResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("socketAuthSecret")]
            public string SocketAuthSecret { get; set; }

            [JsonProperty("authMode")]
            public string AuthMode { get; set; }

            [JsonProperty("authKeyScope")]
            public string AuthKeyScope { get; set; }

            [JsonProperty("error")]
            public string Error { get; set; }
        }

        public static string TryEnroll(
            Guid pluginId,
            Guid departmentId,
            Guid groupId,
            string pluginName,
            string departmentName,
            string groupName,
            string version)
        {
            try
            {
                var request = new
                {
                    pluginId = pluginId.ToString(),
                    pluginName = pluginName ?? string.Empty,
                    departmentId = departmentId.ToString(),
                    departmentName = departmentName ?? string.Empty,
                    groupId = groupId.ToString(),
                    groupName = groupName ?? string.Empty,
                    version = version ?? string.Empty
                };

                var json = JsonConvert.SerializeObject(request);
                using var handler = new System.Net.Http.WinHttpHandler();
                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(8)
                };
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = Constants.DefaultApiBaseUrl.TrimEnd('/') + Constants.SocketAuthEnrollmentPath;
                var response = client.PostAsync(url, content).ConfigureAwait(false).GetAwaiter().GetResult();
                var responseText = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                EnrollmentResponse result = null;
                try
                {
                    result = JsonConvert.DeserializeObject<EnrollmentResponse>(responseText);
                }
                catch
                {
                    // Do not log raw server responses because enrollment responses may contain credentials.
                }

                if (!response.IsSuccessStatusCode || result?.Success != true)
                {
                    var code = result?.Error ?? $"HTTP {(int)response.StatusCode}";
                    PluginContext.Log.Info($"SocketAuthEnrollment :: automatic enrollment unavailable ({code}); legacy fallback remains enabled.");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(result.SocketAuthSecret))
                {
                    PluginContext.Log.Warn("SocketAuthEnrollment :: server returned success without a device key.");
                    return null;
                }

                PluginContext.Log.Info($"SocketAuthEnrollment :: device key received ({result.AuthKeyScope ?? "device-v1"}).");
                return result.SocketAuthSecret;
            }
            catch (Exception ex)
            {
                PluginContext.Log.Info($"SocketAuthEnrollment :: automatic enrollment skipped: {ex.Message}");
                return null;
            }
        }
    }
}
