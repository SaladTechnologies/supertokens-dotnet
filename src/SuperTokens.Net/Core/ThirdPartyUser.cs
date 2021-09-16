using System.Text.Json.Serialization;

namespace SuperTokens.Net.Core
{
    public class ThirdPartyUser
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("thirdParty")]
        public ThirdPartyUserThirdParty ThirdParty { get; set; } = null!;

        [JsonPropertyName("timeJoined")]
        public long TimeJoined { get; set; }

        public class ThirdPartyUserThirdParty
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = null!;

            [JsonPropertyName("userId")]
            public string UserId { get; set; } = null!;
        }
    }
}
