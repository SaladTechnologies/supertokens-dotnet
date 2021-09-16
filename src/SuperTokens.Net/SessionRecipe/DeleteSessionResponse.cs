using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class DeleteSessionResponse
    {
        [JsonPropertyName("sessionHandlesRevoked")]
        public List<string> SessionHandlesRevoked { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
