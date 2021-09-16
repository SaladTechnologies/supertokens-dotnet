using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.SessionRecipe
{
    public class SessionHandlesResponse
    {
        [JsonPropertyName("sessionHandles")]
        public List<string> SessionHandles { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}
