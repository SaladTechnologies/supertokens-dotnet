using System.Text.Json.Serialization;

namespace SuperTokens.TestServer.Models
{
    public sealed class LoginRequest
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;
    }
}
