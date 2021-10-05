using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class KeyInfo
    {
        [JsonPropertyName("publicKey")]
        public string publicKey { get; set; }


        [JsonPropertyName("expiryTime")]
        public long expirationTime { get; set; }

        [JsonPropertyName("createdAt")]
        public long createdAt { get; set; }
    }
}
