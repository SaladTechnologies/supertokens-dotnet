using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace SuperTokens.AspNetCore
{
    public class SuperTokensOptions : AuthenticationSchemeOptions
    {
        public SuperTokensAntiCsrfMode AntiCsrfMode { get; set; }

        public string? ApiBasePath { get; set; }

        public string ApiDomain { get; set; } = null!;

        public string? ApiGatewayPath { get; set; }

        public string AppName { get; set; } = null!;

        public string? CookieDomain { get; set; }

        public SameSiteMode CookieSameSite { get; set; } = SameSiteMode.Unspecified;

        public CookieSecurePolicy CookieSecure { get; set; } = CookieSecurePolicy.SameAsRequest;

        public string CoreAddress { get; set; } = null!;

        public string? CoreApiKey { get; set; }

        public PathString? RefreshPath { get; set; } = "/auth/session/refresh";

        public PathString? SignOutPath { get; set; } = "/auth/signout";

        public string? WebsiteBasePath { get; set; }

        public string WebsiteDomain { get; set; } = null!;
    }
}
