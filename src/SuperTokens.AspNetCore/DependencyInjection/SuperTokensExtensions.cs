using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SuperTokens.AspNetCore;
using SuperTokens.Net;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SuperTokensExtensions
    {
        /// <summary>
        ///     Adds SuperTokens authentication to <see cref="AuthenticationBuilder"/> using the default scheme (
        ///     <see cref="SuperTokensDefaults.AuthenticationScheme"/>).
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddSuperTokens(this AuthenticationBuilder builder) =>
            builder.AddSuperTokens(SuperTokensDefaults.AuthenticationScheme);

        /// <summary>
        ///     Adds SuperTokens authentication to <see cref="AuthenticationBuilder"/> using the specified scheme.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddSuperTokens(this AuthenticationBuilder builder, string authenticationScheme) =>
            builder.AddSuperTokens(authenticationScheme, null, null!);

        /// <summary>
        ///     Adds SuperTokens authentication to <see cref="AuthenticationBuilder"/> using the default scheme (
        ///     <see cref="SuperTokensDefaults.AuthenticationScheme"/>).
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="SuperTokensOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddSuperTokens(this AuthenticationBuilder builder, Action<SuperTokensOptions> configureOptions) =>
            builder.AddSuperTokens(SuperTokensDefaults.AuthenticationScheme, null, configureOptions);

        /// <summary>
        ///     Adds SuperTokens authentication to <see cref="AuthenticationBuilder"/> using the specified scheme.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="SuperTokensOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddSuperTokens(this AuthenticationBuilder builder, string authenticationScheme, Action<SuperTokensOptions> configureOptions) =>
            builder.AddSuperTokens(authenticationScheme, null, configureOptions);

        /// <summary>
        ///     Adds SuperTokens authentication to <see cref="AuthenticationBuilder"/> using the specified scheme.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="displayName">A display name for the authentication handler.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="SuperTokensOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddSuperTokens(this AuthenticationBuilder builder, string authenticationScheme, string? displayName, Action<SuperTokensOptions> configureOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton<IHandshakeContainer, HandshakeContainer>();
            builder.Services.TryAddScoped<ISessionAccessor, SessionAccessor>();
            builder.Services.TryAddScoped<ISessionRecipe, SessionRecipe>();
            builder.Services.AddHttpClient<ICoreApiClient, CoreApiClient>((services, httpClient) =>
            {
                var options = services.GetRequiredService<IOptionsMonitor<SuperTokensOptions>>().Get(authenticationScheme);
                httpClient.BaseAddress = new Uri(options.CoreAddress, UriKind.Absolute);
            });
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<SuperTokensOptions>, SuperTokensPostConfigureOptions>());
            return builder.AddScheme<SuperTokensOptions, SuperTokensHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
