using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class VerifySessionRequest
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("antiCsrfToken")]
        public string? AntiCsrfToken { get; set; }

        [JsonPropertyName("doAntiCsrfCheck")]
        public bool DoAntiCsrfCheck { get; set; }

        [JsonPropertyName("enableAntiCsrf")]
        public bool EnableAntiCsrf { get; set; }
    }
}
