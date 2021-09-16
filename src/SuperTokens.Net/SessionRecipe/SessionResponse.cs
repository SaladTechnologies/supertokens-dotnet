using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class SessionResponse
    {
        [JsonPropertyName("expiry")]
        public long Expiry { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("timeCreated")]
        public long TimeCreated { get; set; }

        [JsonPropertyName("userDataInDatabase")]
        public JsonElement UserDataInDatabase { get; set; }

        [JsonPropertyName("userDataInJWT")]
        public JsonElement UserDataInJwt { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;
    }
}
