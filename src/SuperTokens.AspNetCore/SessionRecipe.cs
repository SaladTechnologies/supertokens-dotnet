using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using SuperTokens.Net;
using SuperTokens.Net.SessionRecipe;

namespace SuperTokens.AspNetCore
{
    public class SessionRecipe : ISessionRecipe
    {
        private readonly ICoreApiClient _coreApiClient;

        private readonly IHandshakeContainer _handshakeContainer;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IOptionsMonitor<SuperTokensOptions> _options;

        private readonly ISessionAccessor _sessionAccessor;

        public SessionRecipe(ICoreApiClient coreApiClient, IHandshakeContainer handshakeContainer, ISessionAccessor sessionAccessor, IHttpContextAccessor httpContextAccessor, IOptionsMonitor<SuperTokensOptions> options)
        {
            _coreApiClient = coreApiClient ?? throw new ArgumentNullException(nameof(coreApiClient));
            _handshakeContainer = handshakeContainer ?? throw new ArgumentNullException(nameof(handshakeContainer));
            _sessionAccessor = sessionAccessor ?? throw new ArgumentNullException(nameof(sessionAccessor));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<SuperTokensSession> AuthenticateAsync(string userId)
        {
            var options = _options.Get(SuperTokensDefaults.AuthenticationScheme);

            var document = JsonDocument.Parse("{}");
            var emptyObject = document.RootElement.Clone();
            document.Dispose();

            var result = await _coreApiClient.CreateSessionAsync(options.CoreApiKey, null, new CreateSessionRequest
            {
                EnableAntiCsrf = options.AntiCsrfMode == SuperTokensAntiCsrfMode.ViaToken,
                UserDataInDatabase = emptyObject,
                UserDataInJwt = emptyObject,
                UserId = userId,
            });
            if (!"OK".Equals(result.Status, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Failed to login the user.");
            }

            await _handshakeContainer.OnHandshakeChanged(result.JwtSigningPublicKeyList, result.JwtSigningPublicKey, DateTimeOffset.FromUnixTimeMilliseconds(result.JwtSigningPublicKeyExpiryTime));

            var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("The session recipe only works in a HTTP context.");
            if (context.Response.HasStarted)
            {
                throw new InvalidOperationException("The session recipe cannot send cookies after the response has started.");
            }

            context.Response.Headers[HeaderNames.SetCookie] = StringValues.Concat(
                context.Response.Headers[HeaderNames.SetCookie],
                new StringValues(new[]
                {
                    new SetCookieHeaderValue(SuperTokensDefaults.AccessTokenCookieName, result.AccessToken.Token)
                    {
                        Domain = options.CookieDomain,
                        Expires = DateTimeOffset.FromUnixTimeMilliseconds(result.AccessToken.Expiry),
                        HttpOnly = true,
                        Path = "/",
                        SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)options.CookieSameSite,
                        Secure = options.CookieSecure == CookieSecurePolicy.Always || (options.CookieSecure == CookieSecurePolicy.SameAsRequest && context.Request.IsHttps),
                    }.ToString(),
                    new SetCookieHeaderValue(SuperTokensDefaults.RefreshTokenCookieName, result.RefreshToken.Token)
                    {
                        Domain = options.CookieDomain,
                        Expires = DateTimeOffset.FromUnixTimeMilliseconds(result.RefreshToken.Expiry),
                        HttpOnly = true,
                        Path = new StringSegment(options.RefreshPath ?? "/"),
                        SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)options.CookieSameSite,
                        Secure = options.CookieSecure == CookieSecurePolicy.Always || (options.CookieSecure == CookieSecurePolicy.SameAsRequest && context.Request.IsHttps),
                    }.ToString(),
                    new SetCookieHeaderValue(SuperTokensDefaults.IdRefreshTokenCookieName, result.IdRefreshToken.Token)
                    {
                        Domain = options.CookieDomain,
                        Expires = DateTimeOffset.FromUnixTimeMilliseconds(result.IdRefreshToken.Expiry),
                        HttpOnly = true,
                        Path = "/",
                        SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)options.CookieSameSite,
                        Secure = options.CookieSecure == CookieSecurePolicy.Always || (options.CookieSecure == CookieSecurePolicy.SameAsRequest && context.Request.IsHttps),
                    }.ToString(),
                }));

            context.Response.Headers[SuperTokensDefaults.FrontTokenHeaderKey] = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                uid = result.Session.UserId,
                ate = result.AccessToken.Expiry,
                up = result.Session.UserDataInJwt,
            })));
            context.Response.Headers[SuperTokensDefaults.IdRefreshTokenHeaderKey] = $"{result.IdRefreshToken.Token};{result.IdRefreshToken.Expiry:D}";
            context.Response.Headers[HeaderNames.AccessControlExposeHeaders] = StringValues.Concat(
                context.Response.Headers[HeaderNames.AccessControlExposeHeaders],
                new StringValues(new[]
                {
                    SuperTokensDefaults.FrontTokenHeaderKey,
                    SuperTokensDefaults.IdRefreshTokenHeaderKey
                }));

            if (result.AntiCsrfToken != null)
            {
                context.Response.Headers[SuperTokensDefaults.AntiCsrfHeaderKey] = result.AntiCsrfToken;
                context.Response.Headers[HeaderNames.AccessControlExposeHeaders] = StringValues.Concat(
                    context.Response.Headers[HeaderNames.AccessControlExposeHeaders],
                    SuperTokensDefaults.AntiCsrfHeaderKey);
            }

            var session = new SuperTokensSession(result.Session.Handle, result.Session.UserId, result.Session.UserDataInJwt.ToString()!);
            _sessionAccessor.Session = session;
            return session;
        }
    }
}
