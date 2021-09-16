using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SuperTokens.Net.SessionRecipe;
using Xunit;

namespace SuperTokens.Net
{
    public class FunctionalTests
    {
        [Fact]
        public async Task Functional()
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://try.supertokens.io", UriKind.Absolute);

            var client = new CoreApiClient(httpClient);
            string apiKey = null!;

            var apiVersions = await client.GetApiVersionAsync(apiKey);
            var cdiVersion = apiVersions.Versions.First();

            var handshake = await client.GetHandshakeAsync(apiKey, cdiVersion);

            var emptyObjectJson = JsonDocument.Parse("{}");
            var emptyObject = emptyObjectJson.RootElement.Clone();
            emptyObjectJson.Dispose();

            var requestObject = new CreateSessionRequest
            {
                EnableAntiCsrf = true,
                UserDataInDatabase = emptyObject,
                UserDataInJwt = emptyObject,
                UserId = new Guid("0de15609-26ce-4652-88de-aa184f40c7ac").ToString("D"),
            };
            var requestContent = JsonSerializer.Serialize(requestObject);

            var session = await client.CreateSessionAsync(apiKey, cdiVersion, requestObject);
        }
    }
}
