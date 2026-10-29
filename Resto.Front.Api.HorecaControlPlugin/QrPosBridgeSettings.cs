using Newtonsoft.Json;

namespace Resto.Front.Api.HorecaControlPlugin
{
    /// <summary>
    /// Optional QR POS Bridge settings in smarthoreca.json.
    /// This is deliberately disabled on existing cash registers.
    /// Keys belong only to a protected local config file, never to client JS.
    /// </summary>
    public sealed class QrPosBridgeSettings
    {
        [JsonProperty("Enabled")]
        public bool Enabled { get; set; } = false;

        [JsonProperty("AllowOrderCreation")]
        public bool AllowOrderCreation { get; set; } = false;

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("PluginIngressToken")]
        public string PluginIngressToken { get; set; }

        [JsonProperty("BridgeToken")]
        public string BridgeToken { get; set; }

        // PIN for a dedicated, least-privileged iikoFront operator with order-creation rights.
        // Local protected config only; never commit a real PIN or log it.
        [JsonProperty("OperatorPin")]
        public string OperatorPin { get; set; }

        [JsonProperty("PollSeconds")]
        public int PollSeconds { get; set; } = 10;
    }
}
