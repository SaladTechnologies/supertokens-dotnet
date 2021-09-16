using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SuperTokens.Net;
using SuperTokens.Net.SessionRecipe;

namespace SuperTokens.AspNetCore
{
    [ApiController]
    [Route("api/login")]
    public sealed class LoginController : ControllerBase
    {
        private readonly ICoreApiClient _coreApiClient;

        public LoginController(ICoreApiClient coreApiClient) =>
            _coreApiClient = coreApiClient ?? throw new ArgumentNullException(nameof(coreApiClient));

        [HttpGet]
        public async Task<ActionResult> Login()
        {
            var document = JsonDocument.Parse("{}");
            var emptyObject = document.RootElement.Clone();
            document.Dispose();

            var session = await _coreApiClient.CreateSessionAsync(null, null, new CreateSessionRequest
            {
                EnableAntiCsrf = false,
                UserDataInDatabase = emptyObject,
                UserDataInJwt = emptyObject,
                UserId = new Guid("97d44afb-e4ea-4feb-b25c-c3bc80c55249").ToString("D"),
            });

            // https://github.com/supertokens/supertokens-node/blob/646c44f7a86536c37fee7279a68be26f7d15ae7b/lib/ts/recipe/session/utils.ts#L215
            this.Response.Cookies.Append(SuperTokensDefaults.AccessTokenCookieName, session.AccessToken.Token);
            this.Response.Cookies.Append(SuperTokensDefaults.IdRefreshTokenCookieName, session.IdRefreshToken.Token);
            this.Response.Cookies.Append(SuperTokensDefaults.RefreshTokenCookieName, session.RefreshToken.Token);
            return this.NoContent();
        }
    }
}
