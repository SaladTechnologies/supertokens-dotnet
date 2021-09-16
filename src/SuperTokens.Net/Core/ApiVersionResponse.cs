using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuperTokens.Net.Core
{
    public class ApiVersionResponse
    {
        [JsonPropertyName("versions")]
        public List<string> Versions { get; set; } = null!;
    }
}
