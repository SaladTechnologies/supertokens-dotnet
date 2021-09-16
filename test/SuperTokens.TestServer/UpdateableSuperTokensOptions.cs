using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SuperTokens.AspNetCore;

namespace SuperTokens.TestServer
{
    public class UpdateableSuperTokensOptions : SuperTokensOptions, IOptionsChangeTokenSource<SuperTokensOptions>
    {
        private CancellationTokenSource? _cancellationTokenSource;

        public string Name { get; } = SuperTokensDefaults.AuthenticationScheme;

        public void Configure(SuperTokensOptions options)
        {
            options.AntiCsrfMode = this.AntiCsrfMode;
            options.ApiBasePath = this.ApiBasePath;
            options.ApiDomain = this.ApiDomain;
            options.ApiGatewayPath = this.ApiGatewayPath;
            options.AppName = this.AppName;
            options.ClaimsIssuer = this.ClaimsIssuer;
            options.CookieDomain = this.CookieDomain;
            options.CookieSameSite = this.CookieSameSite;
            options.CookieSecure = this.CookieSecure;
            options.CoreAddress = this.CoreAddress;
            options.CoreApiKey = this.CoreApiKey;
            options.Events = this.Events;
            options.EventsType = this.EventsType;
            options.ForwardAuthenticate = this.ForwardAuthenticate;
            options.ForwardChallenge = this.ForwardChallenge;
            options.ForwardDefault = this.ForwardDefault;
            options.ForwardDefaultSelector = this.ForwardDefaultSelector;
            options.ForwardForbid = this.ForwardForbid;
            options.ForwardSignIn = this.ForwardSignIn;
            options.ForwardSignOut = this.ForwardSignOut;
            options.RefreshPath = this.RefreshPath;
            options.SignOutPath = this.SignOutPath;
            options.WebsiteBasePath = this.WebsiteBasePath;
            options.WebsiteDomain = this.WebsiteDomain;
        }

        public void FireChangeToken()
        {
            var cts = Interlocked.Exchange(ref _cancellationTokenSource, null);
            cts?.Cancel();
        }

        public IChangeToken GetChangeToken()
        {
            var cts = LazyInitializer.EnsureInitialized(ref _cancellationTokenSource, () => new CancellationTokenSource());
            return new CancellationChangeToken(cts.Token);
        }
    }
}
