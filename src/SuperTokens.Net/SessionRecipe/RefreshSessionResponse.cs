using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class RefreshSessionResponse
    {
        [JsonPropertyName("accessToken")]
        public Core.CookieInfo AccessToken { get; set; } = null!;

        [JsonPropertyName("antiCsrfToken")]
        public string? AntiCsrfToken { get; set; }

        [JsonPropertyName("idRefreshToken")]
        public Core.CookieInfo IdRefreshToken { get; set; } = null!;

        [JsonPropertyName("refreshToken")]
        public Core.CookieInfo RefreshToken { get; set; } = null!;

        [JsonPropertyName("session")]
        public Core.Session Session { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
