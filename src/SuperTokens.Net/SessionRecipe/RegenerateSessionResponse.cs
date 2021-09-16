using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class RegenerateSessionResponse
    {
        [JsonPropertyName("accessToken")]
        public Core.CookieInfo AccessToken { get; set; } = null!;

        [JsonPropertyName("session")]
        public Core.Session Session { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
