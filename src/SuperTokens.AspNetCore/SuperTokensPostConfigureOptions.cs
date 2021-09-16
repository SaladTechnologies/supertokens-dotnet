using System;
using Microsoft.Extensions.Options;

namespace SuperTokens.AspNetCore
{
    public class SuperTokensPostConfigureOptions : IPostConfigureOptions<SuperTokensOptions>
    {
        public void PostConfigure(string name, SuperTokensOptions options)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.CookieDomain == null)
            {
                if (Uri.TryCreate(options.ApiDomain, UriKind.Absolute, out var apiUri))
                {
                    options.CookieDomain = apiUri.Host;
                }
                else
                {
                    throw new InvalidOperationException("The API domain must have a valid host name.");
                }
            }
        }
    }
}
