using System.Text.Json.Serialization;

namespace SuperTokens.Net.Core
{
    public class User
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("timeJoined")]
        public long TimeJoined { get; set; }
    }
}
