using System.Text.Json.Serialization;

namespace SuperTokens.Net.Core
{
    public class CookieInfo
    {
        [JsonPropertyName("createdTime")]
        public long CreatedTime { get; set; }

        [JsonPropertyName("expiry")]
        public long Expiry { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;
    }
}
