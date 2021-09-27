using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class VerifySessionResponse
    {
        [JsonPropertyName("accessToken")]
        public Core.CookieInfo? AccessToken { get; set; }

        [JsonPropertyName("jwtSigningPublicKeyList")]
        public List<KeyInfo>? JwtSigningPublicKeyList { get; set; } = null;

        [JsonPropertyName("jwtSigningPublicKey")]
        public string JwtSigningPublicKey { get; set; } = null!;

        [JsonPropertyName("jwtSigningPublicKeyExpiryTime")]
        public long JwtSigningPublicKeyExpiryTime { get; set; }

        [JsonPropertyName("session")]
        public Core.Session Session { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
