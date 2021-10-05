using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class CreateSessionResponse
    {
        [JsonPropertyName("accessToken")]
        public Core.CookieInfo AccessToken { get; set; } = null!;

        [JsonPropertyName("antiCsrfToken")]
        public string? AntiCsrfToken { get; set; }

        [JsonPropertyName("idRefreshToken")]
        public Core.CookieInfo IdRefreshToken { get; set; } = null!;

        [JsonPropertyName("jwtSigningPublicKeyList")]
        public List<KeyInfo>? JwtSigningPublicKeyList { get; set; } = null;

        [JsonPropertyName("jwtSigningPublicKey")]
        public string JwtSigningPublicKey { get; set; } = null!;

        [JsonPropertyName("jwtSigningPublicKeyExpiryTime")]
        public long JwtSigningPublicKeyExpiryTime { get; set; }

        [JsonPropertyName("refreshToken")]
        public Core.CookieInfo RefreshToken { get; set; } = null!;

        [JsonPropertyName("session")]
        public Core.Session Session { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
