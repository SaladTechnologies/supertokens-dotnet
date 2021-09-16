using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class RegenerateSessionRequest
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("userDataInJWT")]
        public JsonElement UserDataInJwt { get; set; }
    }
}
