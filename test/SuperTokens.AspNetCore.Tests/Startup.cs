using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SuperTokens.AspNetCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // ---------- ASP.NET Core Authentication ----------

            services
                .AddAuthentication("SuperTokens")
                .AddSuperTokens(options =>
                {
                    options.ApiDomain = "https://login.salad.com";
                    options.AppName = "Salad";
                    options.CoreAddress = "http://127.0.0.1:3567";
                    options.WebsiteDomain = "https://salad.com";
                });

            // ---------- ASP.NET Core MVC ----------

            services
                .AddHttpContextAccessor()
                .AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use((context, next) =>
            {
                if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
                {
                    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                }

                if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
                {
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                }

                if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                }

                if (!context.Response.Headers.ContainsKey("X-Xss-Protection"))
                {
                    context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                }

                return next();
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
