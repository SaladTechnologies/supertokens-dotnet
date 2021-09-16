using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class CreateSessionRequest
    {
        [JsonPropertyName("enableAntiCsrf")]
        public bool EnableAntiCsrf { get; set; }

        [JsonPropertyName("userDataInDatabase")]
        public JsonElement UserDataInDatabase { get; set; }

        [JsonPropertyName("userDataInJWT")]
        public JsonElement UserDataInJwt { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;
    }
}
