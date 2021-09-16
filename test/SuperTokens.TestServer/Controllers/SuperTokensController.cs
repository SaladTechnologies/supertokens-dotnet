using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SuperTokens.AspNetCore;
using SuperTokens.TestServer.Models;

namespace SuperTokens.TestServer.Controllers
{
    [ApiController]
    public class SuperTokensController : ControllerBase
    {
        private readonly Counters _counters;

        private readonly UpdateableSuperTokensOptions _optionsSource;

        private readonly ISessionAccessor _sessionAccessor;

        private readonly ISessionRecipe _sessionRecipe;

        public SuperTokensController(UpdateableSuperTokensOptions optionsSource, ISessionAccessor sessionAccessor, ISessionRecipe sessionRecipe, Counters counters)
        {
            _optionsSource = optionsSource ?? throw new ArgumentNullException(nameof(optionsSource));
            _sessionAccessor = sessionAccessor ?? throw new ArgumentNullException(nameof(sessionAccessor));
            _sessionRecipe = sessionRecipe ?? throw new ArgumentNullException(nameof(sessionRecipe));
            _counters = counters ?? throw new ArgumentNullException(nameof(counters));
        }

        [AllowAnonymous]
        [HttpPost("/beforeeach")]
        public ActionResult BeforeEach() =>
            this.StatusCode(StatusCodes.Status200OK);

        [AllowAnonymous]
        [HttpPost("/checkAllowCredentials")]
        public ActionResult<bool> CheckAllowCredentials() =>
            this.Content(!string.IsNullOrEmpty(this.Request.Headers["allow-credentials"]) ? "true" : "false", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [Authorize]
        [HttpGet("/check-rid")]
        public ActionResult<string> CheckRid() =>
            this.Content(string.IsNullOrEmpty(this.Request.Headers["rid"]) ? "fail" : "success", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [Authorize]
        [HttpGet("/")]
        public ActionResult<string> Get() =>
            this.Content(_sessionAccessor.Session?.UserId ?? "", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [Authorize]
        [HttpGet("/update-jwt")]
        public ActionResult<string> GetJwt() =>
            this.Content(_sessionAccessor.Session?.UserDataInJwt ?? "", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [AllowAnonymous]
        [HttpGet("/getSessionCalledTime")]
        public ActionResult<string> GetSessionCalledTime() =>
            this.Content($"{_counters.NoOfTimesGetSessionCalledDuringTest:D}", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [AllowAnonymous]
        [HttpPost("/login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest body)
        {
            var session = await _sessionRecipe.AuthenticateAsync(body.UserId);
            return this.Content(session.UserId, new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
        }

        [Authorize]
        [HttpPost("/logout")]
        public async Task<ActionResult<string>> Logout()
        {
            await this.HttpContext.SignOutAsync(SuperTokensDefaults.AuthenticationScheme);
            return this.Content("success", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
        }

        [AllowAnonymous]
        [HttpPost("/multipleInterceptors")]
        public ActionResult MultipleInterceptors() =>
            this.Content(string.IsNullOrEmpty(this.Request.Headers["interceptorheader1"]) && string.IsNullOrEmpty(this.Request.Headers["interceptorheader2"]) ? "failure" : "success", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [AllowAnonymous]
        [HttpGet("/ping")]
        public ActionResult<string> Ping() =>
            this.Content("success", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [AllowAnonymous]
        [HttpGet("/refreshAttemptedTime")]
        public ActionResult<string> RefreshAttemptedTime() =>
            this.Content($"{_counters.NoOfTimesRefreshAttemptedDuringTest:D}", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [AllowAnonymous]
        [HttpGet("/refreshCalledTime")]
        public ActionResult<string> RefreshCalledTime() =>
            this.Content($"{_counters.NoOfTimesRefreshCalledDuringTest:D}", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));

        [Authorize]
        [HttpPost("/revokeAll")]
        public ActionResult<string> RevokeAll()
        {
            // TODO: Implement.
            throw new NotImplementedException();
        }

        [AllowAnonymous]
        [HttpPost("/setAntiCsrf")]
        public ActionResult<string> SetAntiCsrf([FromBody] SetAntiCsrfRequest body)
        {
            if (body.EnableAntiCsrf.HasValue)
            {
                if (body.EnableAntiCsrf.Value)
                {
                    _optionsSource.AntiCsrfMode = SuperTokensAntiCsrfMode.ViaToken;
                }
                else
                {
                    _optionsSource.AntiCsrfMode = SuperTokensAntiCsrfMode.None;
                }

                _optionsSource.FireChangeToken();
            }

            return this.Content("success", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
        }

        [AllowAnonymous]
        [HttpGet("/testError")]
        public ActionResult TestError()
        {
            var content = this.Content("test error message", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
            content.StatusCode = StatusCodes.Status500InternalServerError;
            return content;
        }

        [AllowAnonymous]
        [HttpGet("/testHeader")]
        public ActionResult<string> TestHeader() =>
            this.Ok(new
            {
                success = !string.IsNullOrEmpty(this.Request.Headers["st-custom-header"]),
            });

        [AllowAnonymous]
        [HttpDelete("/testing")]
        [HttpGet("/testing")]
        [HttpHead("/testing")]
        [HttpPatch("/testing")]
        [HttpPost("/testing")]
        [HttpPut("/testing")]
        public ActionResult<string> Testing()
        {
            var testHeader = this.Request.Headers["testing"];
            if (!string.IsNullOrEmpty(testHeader))
            {
                this.Response.Headers["testing"] = testHeader;
            }

            return this.Content("success", new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
        }

        [AllowAnonymous]
        [HttpPost("/testUserConfig")]
        public ActionResult TestUserConfig() =>
            this.StatusCode(StatusCodes.Status200OK);

        [Authorize]
        [HttpPost("/update-jwt")]
        public ActionResult<string> UpdateJwt()
        {
            // TODO: Implement.
            throw new NotImplementedException();
        }
    }
}
