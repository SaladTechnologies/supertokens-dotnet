using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class RefreshSessionRequest
    {
        [JsonPropertyName("antiCsrfToken")]
        public string? AntiCsrfToken { get; set; }

        [JsonPropertyName("enableAntiCsrf")]
        public bool EnableAntiCsrf { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = null!;
    }
}
