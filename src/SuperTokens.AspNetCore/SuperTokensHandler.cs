using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using SuperTokens.AspNetCore.Events;
using SuperTokens.Net;
using SuperTokens.Net.Core;
using SuperTokens.Net.SessionRecipe;

namespace SuperTokens.AspNetCore
{
    public class SuperTokensHandler : SignOutAuthenticationHandler<SuperTokensOptions>, IAuthenticationRequestHandler
    {
        private readonly ICoreApiClient _coreApiClient;

        private readonly IHandshakeContainer _handshakeContainer;

        private readonly ISessionAccessor _sessionAccessor;

        public SuperTokensHandler(
            ICoreApiClient coreApiClient,
            IHandshakeContainer handshakeContainer,
            ISessionAccessor sessionAccessor,
            IOptionsMonitor<SuperTokensOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) :
            base(options, logger, encoder, clock)
        {
            _coreApiClient = coreApiClient ?? throw new ArgumentNullException(nameof(coreApiClient));
            _handshakeContainer = handshakeContainer ?? throw new ArgumentNullException(nameof(handshakeContainer));
            _sessionAccessor = sessionAccessor ?? throw new ArgumentNullException(nameof(sessionAccessor));
        }

        protected new SuperTokensEvents Events
        {
            get => (SuperTokensEvents)base.Events!;
            set => base.Events = value;
        }

        public Task<bool> HandleRequestAsync()
        {
            if (this.Options.RefreshPath.HasValue && this.Options.RefreshPath == this.Request.Path && HttpMethods.IsPost(this.Request.Method))
            {
                return this.HandleRefreshRequestAsync();
            }
            else if (this.Options.SignOutPath.HasValue && this.Options.SignOutPath == this.Request.Path && HttpMethods.IsPost(this.Request.Method))
            {
                return this.HandleSignOutRequestAsync();
            }

            return TaskUtilities.FalseTask;
        }

        protected override Task<object> CreateEventsAsync() =>
            Task.FromResult<object>(new SuperTokensEvents());

        protected virtual string? GetAccessTokenFromCookie() =>
                    !this.Request.Cookies.TryGetValue(SuperTokensDefaults.AccessTokenCookieName, out var token) || string.IsNullOrEmpty(token)
                ? null
                : token;

        protected virtual string? GetAntiCsrfTokenFromHeader() =>
            !this.Request.Headers.TryGetValue(SuperTokensDefaults.AntiCsrfHeaderKey, out var values) || values.Count != 1 || string.IsNullOrEmpty(values[0])
                ? null
                : values[0];

        protected virtual string? GetIdRefreshTokenFromCookie() =>
            !this.Request.Cookies.TryGetValue(SuperTokensDefaults.IdRefreshTokenCookieName, out var token) || string.IsNullOrEmpty(token)
                ? null
                : token;

        protected virtual string? GetRecipeIdFromHeader() =>
            !this.Request.Headers.TryGetValue(SuperTokensDefaults.RecipeIdHeaderKey, out var values) || values.Count != 1 || string.IsNullOrEmpty(values[0])
                ? null
                : values[0];

        protected virtual string? GetRefreshTokenFromCookie() =>
            !this.Request.Cookies.TryGetValue(SuperTokensDefaults.RefreshTokenCookieName, out var token) || string.IsNullOrEmpty(token)
                ? null
                : token;

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (_sessionAccessor.Session != null)
            {
                return AuthenticateResult.Success(new AuthenticationTicket(this.CreatePrincipal(_sessionAccessor.Session.UserId), this.Scheme.Name));
            }

            var now = this.Clock.UtcNow;

            var messageReceivedContext = new MessageReceivedContext(this.Context, this.Scheme, this.Options);
            await this.Events.MessageReceived(messageReceivedContext);
            if (messageReceivedContext.Result != null)
            {
                return messageReceivedContext.Result;
            }

            var idRefreshToken = this.GetIdRefreshTokenFromCookie();
            if (idRefreshToken == null)
            {
                return AuthenticateResult.NoResult();
            }

            var accessToken = this.GetAccessTokenFromCookie();
            if (accessToken == null)
            {
                await this.SendTryRefreshTokenResponse();
                return AuthenticateResult.Fail("The access token expired.");
            }

            string? antiCsrfToken = null;
            if (!HttpMethods.IsGet(this.Request.Method))
            {
                if (this.Options.AntiCsrfMode == SuperTokensAntiCsrfMode.ViaCustomHeader)
                {
                    var recipeId = this.GetRecipeIdFromHeader();
                    if (recipeId == null)
                    {
                        await this.SendTryRefreshTokenResponse();
                        return AuthenticateResult.Fail("The anti-CSRF header is missing.");
                    }
                }
                else if (this.Options.AntiCsrfMode == SuperTokensAntiCsrfMode.ViaToken)
                {
                    antiCsrfToken = this.GetAntiCsrfTokenFromHeader();
                    if (antiCsrfToken == null)
                    {
                        await this.SendTryRefreshTokenResponse();
                        return AuthenticateResult.Fail("The anti-CSRF header is missing.");
                    }

                    // We will check the token value after parsing & validating the access token.
                }
            }

            var handshake = await _handshakeContainer.GetHandshakeAsync(this.Options.CoreApiKey, null, this.Context.RequestAborted);
            AccessToken? parsedAccessToken;

            // If we have no key old enough to verify this access token we should reject it without calling the core

            if (!JwtUtilities.TryParse(accessToken, out var jwtPayload, out var components) ||
                !AccessTokenUtilities.TryParse(jwtPayload, out parsedAccessToken))
            {
                await this.SendTryRefreshTokenResponse();
                return AuthenticateResult.Fail("The access token is invalid.");
            }

            if (parsedAccessToken.ExpiryTime < now)
            {
                await this.SendTryRefreshTokenResponse();
                return AuthenticateResult.Fail("The access token expired.");
            }

            var foundSigningKeyOlderThanToken = false;
            var isSignatureValid = false;
            foreach (var keyInfo in handshake.GetAccessTokenSigningPublicKeyList(now))
            {
                if (JwtUtilities.Validate(components, keyInfo.PublicKey))
                {
                    // If we reached a key older than the token then we don't need to try older keys since
                    // the keys are always signed with the latest available key
                    // The keylist in the handshake is ordered from newest to oldest.
                    isSignatureValid = true;
                }
                if (keyInfo.Creation < parsedAccessToken.TimeCreated)
                {
                    foundSigningKeyOlderThanToken = true;
                    break;
                }
            }

            // If the token was created before the oldest key in the cache but hasn't expired, then a config value must've changed.
            // E.g., the access_token_signing_key_update_interval was reduced, or access_token_signing_key_dynamic was turned on.
            // Either way, the user needs to refresh the access token as validating by the server is likely to do nothing.
            if (!foundSigningKeyOlderThanToken)
            {
                await this.SendTryRefreshTokenResponse();
                return AuthenticateResult.Fail("The access token expired.");
            }

            if (antiCsrfToken != null && !antiCsrfToken.Equals(parsedAccessToken.AntiCsrfToken, StringComparison.Ordinal))
            {
                await this.SendTryRefreshTokenResponse();
                return AuthenticateResult.Fail("The anti-CSRF header is invalid.");
            }

            if (isSignatureValid && !handshake.AccessTokenBlacklistingEnabled && parsedAccessToken.ParentRefreshTokenHash1 == null)
            {
                _sessionAccessor.Session = new SuperTokensSession(parsedAccessToken.SessionHandle, parsedAccessToken.UserId, parsedAccessToken.UserData);
                return AuthenticateResult.Success(new AuthenticationTicket(this.CreatePrincipal(_sessionAccessor.Session.UserId), this.Scheme.Name));
            }

            try
            {
                var result = await _coreApiClient.VerifySessionAsync(
                    this.Options.CoreApiKey,
                    null,
                    new VerifySessionRequest
                    {
                        AccessToken = accessToken,
                        AntiCsrfToken = antiCsrfToken,
                        DoAntiCsrfCheck = this.Options.AntiCsrfMode != SuperTokensAntiCsrfMode.ViaToken,
                        EnableAntiCsrf = this.Options.AntiCsrfMode == SuperTokensAntiCsrfMode.ViaToken,
                    },
                    this.Context.RequestAborted);

                if (!string.IsNullOrEmpty(result.JwtSigningPublicKey))
                {
                    await _handshakeContainer.OnHandshakeChanged(result.JwtSigningPublicKeyList, result.JwtSigningPublicKey, DateTimeOffset.FromUnixTimeMilliseconds(result.JwtSigningPublicKeyExpiryTime));
                }

                if ("TRY_REFRESH_TOKEN".Equals(result.Status, StringComparison.Ordinal))
                {
                    await this.SendTryRefreshTokenResponse();
                    return AuthenticateResult.Fail("The access token expired.");
                }
                else if ("UNAUTHORISED".Equals(result.Status, StringComparison.Ordinal))
                {
                    await this.SendUnauthorisedResponse();
                    return AuthenticateResult.Fail("The access token expired.");
                }

                if (result.AccessToken != null)
                {
                    this.RefreshSessionCookies(result.Session, result.AccessToken);
                }

                _sessionAccessor.Session = new(result.Session.Handle, result.Session.UserId, result.Session.UserDataInJwt.GetRawText());
                return AuthenticateResult.Success(new AuthenticationTicket(this.CreatePrincipal(result.Session.UserId), this.Scheme.Name));
            }
            catch (CoreApiResponseException e)
            {
                this.Logger.LogError(e, "Failed to verify SuperTokens session.");
                await this.WriteEmptyResponse(StatusCodes.Status500InternalServerError);
                return AuthenticateResult.NoResult();
            }
            catch (HttpRequestException e)
            {
                this.Logger.LogError(e, "Failed to verify SuperTokens session.");
                await this.WriteEmptyResponse(StatusCodes.Status500InternalServerError);
                return AuthenticateResult.NoResult();
            }
        }

        protected override async Task HandleSignOutAsync(AuthenticationProperties? properties)
        {
            var result = await this.AuthenticateAsync();
            if (result.Succeeded)
            {
                await _coreApiClient.DeleteSessionAsync(this.Options.CoreApiKey, null, new DeleteSessionRequest
                {
                    SessionHandles = new() { _sessionAccessor.Session!.Handle },
                });
                this.DeleteSessionCookies();
            }
            else if (result.None)
            {
                await this.SendTryRefreshTokenResponse();
            }
        }

        protected override Task InitializeHandlerAsync()
        {
            this.Context.Response.OnStarting(this.FinishResponseAsync);
            return base.InitializeHandlerAsync();
        }

        private void AddSessionCookies(Session session, CookieInfo accessToken, CookieInfo refreshToken, CookieInfo idRefreshToken, string? antiCsrfToken)
        {
            if (this.Response.HasStarted)
            {
                throw new InvalidOperationException("Response headers have already been sent to the client");
            }

            this.Response.Headers[HeaderNames.SetCookie] = StringValues.Concat(
                this.Response.Headers[HeaderNames.SetCookie],
                new StringValues(new[]
                {
                    new SetCookieHeaderValue(SuperTokensDefaults.AccessTokenCookieName, accessToken.Token)
                    {
                        Domain = this.Options.CookieDomain,
                        Expires = DateTimeOffset.FromUnixTimeMilliseconds(accessToken.Expiry),
                        HttpOnly = true,
                        Path = "/",
                        SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)this.Options.CookieSameSite,
                        Secure = this.Options.CookieSecure == CookieSecurePolicy.Always || (this.Options.CookieSecure == CookieSecurePolicy.SameAsRequest && this.Request.IsHttps),
                    }.ToString(),
                    new SetCookieHeaderValue(SuperTokensDefaults.RefreshTokenCookieName, refreshToken.Token)
                    {
                        Domain = this.Options.CookieDomain,
                        Expires = DateTimeOffset.FromUnixTimeMilliseconds(refreshToken.Expiry),
                        HttpOnly = true,
                        Path = (this.Options.RefreshPath ?? "/").Value,
                        SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)this.Options.CookieSameSite,
                        Secure = this.Options.CookieSecure == CookieSecurePolicy.Always || (this.Options.CookieSecure == CookieSecurePolicy.SameAsRequest && this.Request.IsHttps),
                    }.ToString(),
                    new SetCookieHeaderValue(SuperTokensDefaults.IdRefreshTokenCookieName, idRefreshToken.Token)
                    {
                        Domain = this.Options.CookieDomain,
                        Expires = DateTimeOffset.FromUnixTimeMilliseconds(idRefreshToken.Expiry),
                        HttpOnly = true,
                        Path = "/",
                        SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)this.Options.CookieSameSite,
                        Secure = this.Options.CookieSecure == CookieSecurePolicy.Always || (this.Options.CookieSecure == CookieSecurePolicy.SameAsRequest && this.Request.IsHttps),
                    }.ToString(),
                }));

            this.Response.Headers[SuperTokensDefaults.FrontTokenHeaderKey] = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                uid = session.UserId,
                ate = accessToken.Expiry,
                up = session.UserDataInJwt,
            })));
            this.Response.Headers[SuperTokensDefaults.IdRefreshTokenHeaderKey] = $"{idRefreshToken.Token};{idRefreshToken.Expiry:D}";
            this.Response.Headers[HeaderNames.AccessControlExposeHeaders] = StringValues.Concat(
                this.Response.Headers[HeaderNames.AccessControlExposeHeaders],
                new StringValues(new[]
                {
                    SuperTokensDefaults.FrontTokenHeaderKey,
                    SuperTokensDefaults.IdRefreshTokenHeaderKey
                }));

            if (antiCsrfToken != null)
            {
                this.Response.Headers[SuperTokensDefaults.AntiCsrfHeaderKey] = antiCsrfToken;
                this.Response.Headers[HeaderNames.AccessControlExposeHeaders] = StringValues.Concat(
                    this.Response.Headers[HeaderNames.AccessControlExposeHeaders],
                    SuperTokensDefaults.AntiCsrfHeaderKey);
            }
        }

        private ClaimsPrincipal CreatePrincipal(string userId) =>
            new GenericPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String),
                    },
                    this.Scheme.Name),
                null);

        private void DeleteSessionCookies()
        {
            if (this.Response.HasStarted)
            {
                throw new InvalidOperationException("Response headers have already been sent to the client");
            }

            this.Response.Cookies.Delete(SuperTokensDefaults.AccessTokenCookieName, new CookieOptions
            {
                Domain = this.Options.CookieDomain,
                HttpOnly = true,
                IsEssential = true,
                Path = "/",
                SameSite = this.Options.CookieSameSite,
                Secure = this.Options.CookieSecure == CookieSecurePolicy.Always || (this.Options.CookieSecure == CookieSecurePolicy.SameAsRequest && this.Request.IsHttps),
            });
            this.Response.Cookies.Delete(SuperTokensDefaults.RefreshTokenCookieName, new CookieOptions
            {
                Domain = this.Options.CookieDomain,
                HttpOnly = true,
                IsEssential = true,
                Path = this.Options.RefreshPath ?? "/",
                SameSite = this.Options.CookieSameSite,
                Secure = this.Options.CookieSecure == CookieSecurePolicy.Always || (this.Options.CookieSecure == CookieSecurePolicy.SameAsRequest && this.Request.IsHttps),
            });
            this.Response.Cookies.Delete(SuperTokensDefaults.IdRefreshTokenCookieName, new CookieOptions
            {
                Domain = this.Options.CookieDomain,
                HttpOnly = true,
                IsEssential = true,
                Path = "/",
                SameSite = this.Options.CookieSameSite,
                Secure = this.Options.CookieSecure == CookieSecurePolicy.Always || (this.Options.CookieSecure == CookieSecurePolicy.SameAsRequest && this.Request.IsHttps),
            });
            this.Response.Headers[SuperTokensDefaults.IdRefreshTokenHeaderKey] = "remove";
            this.Response.Headers[HeaderNames.AccessControlExposeHeaders] = StringValues.Concat(this.Response.Headers[HeaderNames.AccessControlExposeHeaders], SuperTokensDefaults.IdRefreshTokenHeaderKey);
        }

        private Task FinishResponseAsync() =>
            Task.CompletedTask;

        private async Task<bool> HandleRefreshRequestAsync()
        {
            var idRefreshToken = this.GetIdRefreshTokenFromCookie();
            if (idRefreshToken == null)
            {
                // Do not delete cookies because of a possible race condition
                // (https://github.com/supertokens/supertokens-node/issues/17).
                await this.SendUnauthorisedResponse();
                return true;
            }

            var refreshToken = this.GetRefreshTokenFromCookie();
            if (refreshToken == null)
            {
                this.DeleteSessionCookies();
                await this.SendUnauthorisedResponse();
                return true;
            }

            string? antiCsrfToken = null;
            if (this.Options.AntiCsrfMode == SuperTokensAntiCsrfMode.ViaCustomHeader)
            {
                var recipeId = this.GetRecipeIdFromHeader();
                if (recipeId == null)
                {
                    this.DeleteSessionCookies();
                    await this.SendUnauthorisedResponse();
                    return true;
                }
            }
            else if (this.Options.AntiCsrfMode == SuperTokensAntiCsrfMode.ViaToken)
            {
                antiCsrfToken = this.GetAntiCsrfTokenFromHeader();
                if (antiCsrfToken == null)
                {
                    this.DeleteSessionCookies();
                    await this.SendUnauthorisedResponse();
                    return true;
                }

                // We will check the token value on the server.
            }

            try
            {
                var result = await _coreApiClient.RefreshSessionAsync(
                    this.Options.CoreApiKey,
                    null,
                    new RefreshSessionRequest
                    {
                        AntiCsrfToken = antiCsrfToken,
                        EnableAntiCsrf = this.Options.AntiCsrfMode == SuperTokensAntiCsrfMode.ViaToken,
                        RefreshToken = refreshToken,
                    },
                    this.Context.RequestAborted);

                if ("TOKEN_THEFT_DETECTED".Equals(result.Status, StringComparison.Ordinal))
                {
                    this.DeleteSessionCookies();
                    await this.SendTokenTheftDetectedResponse();
                    return true;
                }
                else if ("UNAUTHORISED".Equals(result.Status, StringComparison.Ordinal))
                {
                    this.DeleteSessionCookies();
                    await this.SendUnauthorisedResponse();
                    return true;
                }

                _sessionAccessor.Session = new(result.Session.Handle, result.Session.UserId, result.Session.UserDataInJwt.GetRawText());

                this.AddSessionCookies(result.Session, result.AccessToken, result.RefreshToken, result.IdRefreshToken, result.AntiCsrfToken);
                await this.SendOkResponse();
                return true;
            }
            catch (CoreApiResponseException e)
            {
                this.Logger.LogError(e, "Failed to refresh SuperTokens session.");
                await this.WriteEmptyResponse(StatusCodes.Status500InternalServerError);
                return true;
            }
            catch (HttpRequestException e)
            {
                this.Logger.LogError(e, "Failed to refresh SuperTokens session.");
                await this.WriteEmptyResponse(StatusCodes.Status500InternalServerError);
                return true;
            }
        }

        private async Task<bool> HandleSignOutRequestAsync()
        {
            var result = await this.AuthenticateAsync();
            if (result.Succeeded)
            {
                await _coreApiClient.DeleteSessionAsync(this.Options.CoreApiKey, null, new DeleteSessionRequest
                {
                    SessionHandles = new() { _sessionAccessor.Session!.Handle },
                });
                this.DeleteSessionCookies();
                await this.SendOkResponse();
            }
            else if (result.None)
            {
                await this.SendTryRefreshTokenResponse();
            }
            else
            {
                await this.SendUnauthorisedResponse();
            }

            return true;
        }

        private void RefreshSessionCookies(Session session, CookieInfo accessToken)
        {
            if (this.Response.HasStarted)
            {
                throw new InvalidOperationException("Response headers have already been sent to the client");
            }

            this.Response.Headers[HeaderNames.SetCookie] = StringValues.Concat(
                this.Response.Headers[HeaderNames.SetCookie],
                new SetCookieHeaderValue(SuperTokensDefaults.AccessTokenCookieName, accessToken.Token)
                {
                    Domain = this.Options.CookieDomain,
                    Expires = DateTimeOffset.FromUnixTimeMilliseconds(accessToken.Expiry),
                    HttpOnly = true,
                    Path = "/",
                    SameSite = (Microsoft.Net.Http.Headers.SameSiteMode)this.Options.CookieSameSite,
                    Secure = this.Options.CookieSecure == CookieSecurePolicy.Always || (this.Options.CookieSecure == CookieSecurePolicy.SameAsRequest && this.Request.IsHttps),
                }.ToString());
            this.Response.Headers[SuperTokensDefaults.FrontTokenHeaderKey] = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                uid = session.UserId,
                ate = accessToken.Expiry,
                up = session.UserDataInJwt,
            })));
            this.Response.Headers[HeaderNames.AccessControlExposeHeaders] = StringValues.Concat(this.Response.Headers[HeaderNames.AccessControlExposeHeaders], SuperTokensDefaults.FrontTokenHeaderKey);
        }

        private Task SendOkResponse() =>
            this.WriteJsonResponse(StatusCodes.Status200OK, "{}");

        private Task SendTokenTheftDetectedResponse() =>
            this.WriteErrorResponse("token theft detected");

        private Task SendTryRefreshTokenResponse() =>
            this.WriteErrorResponse("try refresh token");

        private Task SendUnauthorisedResponse() =>
            this.WriteErrorResponse("unauthorised");

        private Task WriteEmptyResponse(int statusCode)
        {
            if (this.Response.HasStarted)
            {
                throw new InvalidOperationException("Response headers have already been sent to the client");
            }

            this.Response.StatusCode = statusCode;

            return this.Response.CompleteAsync();
        }

        private Task WriteErrorResponse(string message) =>
            this.WriteJsonResponse(
                StatusCodes.Status401Unauthorized,
                JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = message,
                }));

        private async Task WriteJsonResponse(int statusCode, string json)
        {
            if (this.Response.HasStarted)
            {
                throw new InvalidOperationException("Response headers have already been sent to the client");
            }

            this.Response.StatusCode = statusCode;

            using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            foreach (var header in content.Headers)
            {
                this.Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
            }

            await content.CopyToAsync(this.Response.Body, this.Context.RequestAborted).ConfigureAwait(false);
            await this.Response.CompleteAsync();
        }

        private class ErrorResponse
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }
    }
}
