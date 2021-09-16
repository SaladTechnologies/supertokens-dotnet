using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SuperTokens.AspNetCore;

namespace SuperTokens.TestServer
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) =>
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseMiddleware<CountersMiddleware>();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // ---------- Options ----------

            var updateableOptions = new UpdateableSuperTokensOptions();
            _configuration.GetSection("SuperTokens").Bind(updateableOptions);
            services.AddSingleton(updateableOptions);
            services.AddSingleton<IOptionsChangeTokenSource<SuperTokensOptions>>(updateableOptions);

            // ---------- ASP.NET Core Authentication ----------

            services
                .AddAuthentication()
                .AddSuperTokens(updateableOptions.Configure);

            // ---------- ASP.NET Core Authorization ----------

            services
                .AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder(SuperTokensDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                });

            // ---------- ASP.NET Core CORS ----------

            if (!updateableOptions.ApiDomain.Equals(updateableOptions.WebsiteDomain))
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.WithOrigins(updateableOptions.WebsiteDomain)
                            .WithMethods(
                                HttpMethods.Delete,
                                HttpMethods.Get,
                                HttpMethods.Post,
                                HttpMethods.Put)
                            .WithHeaders(
                                HeaderNames.ContentType,
                                SuperTokensDefaults.AntiCsrfHeaderKey,
                                SuperTokensDefaults.FrontendDriverInterfaceHeaderKey,
                                SuperTokensDefaults.RecipeIdHeaderKey)
                            .AllowCredentials();
                    });
                });
            }

            // ---------- ASP.NET Core MVC ----------

            services
                .AddHttpContextAccessor()
                .AddControllersWithViews();

            // ---------- Custom Services ----------

            services.AddSingleton<Counters>();
        }
    }
}
