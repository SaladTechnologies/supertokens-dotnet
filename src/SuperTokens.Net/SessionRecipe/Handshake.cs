using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class Handshake
    {
        [JsonPropertyName("accessTokenBlacklistingEnabled")]
        public bool AccessTokenBlacklistingEnabled { get; set; }

        [JsonPropertyName("accessTokenValidity")]
        public long AccessTokenValidity { get; set; }

        [JsonPropertyName("jwtSigningPublicKeyList")]
        public List<KeyInfo>? JwtSigningPublicKeyList { get; set; } = null;

        [JsonPropertyName("jwtSigningPublicKey")]
        public string JwtSigningPublicKey { get; set; } = null!;

        [JsonPropertyName("jwtSigningPublicKeyExpiryTime")]
        public long JwtSigningPublicKeyExpiryTime { get; set; }

        [JsonPropertyName("refreshTokenValidity")]
        public long RefreshTokenValidity { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
