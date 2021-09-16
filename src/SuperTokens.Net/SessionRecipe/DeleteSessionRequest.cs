using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class DeleteSessionRequest
    {
        [JsonPropertyName("sessionHandles")]
        public List<string>? SessionHandles { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }
}
