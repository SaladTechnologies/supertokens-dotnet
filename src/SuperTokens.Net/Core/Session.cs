using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.Core
{
    public class Session
    {
        [JsonPropertyName("handle")]
        public string Handle { get; set; } = null!;

        [JsonPropertyName("userDataInJWT")]
        public JsonElement UserDataInJwt { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;
    }
}
