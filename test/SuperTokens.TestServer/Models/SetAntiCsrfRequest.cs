using System.Text.Json.Serialization;

namespace SuperTokens.TestServer.Models
{
    public sealed class SetAntiCsrfRequest
    {
        [JsonPropertyName("enableAntiCsrf")]
        public bool? EnableAntiCsrf { get; set; }
    }
}
