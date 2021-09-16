using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace SuperTokens.AspNetCore
{
    public class IntegrationTests : IClassFixture<IntegrationTests.TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public IntegrationTests(TestWebApplicationFactory factory) =>
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        [Fact]
        public async Task FunctionalTests()
        {
            using var client = _factory.CreateClient();

            {
                using var loginResponse = await client.GetAsync("/api/login");
                Assert.True(loginResponse.IsSuccessStatusCode);
            }

            {
                using var valuesResponse = await client.GetAsync("/api/values");
                Assert.True(valuesResponse.IsSuccessStatusCode);
            }

            {
                using var refreshContent = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
                using var refreshResponse = await client.PostAsync("/auth/session/refresh", refreshContent);
                Assert.True(refreshResponse.IsSuccessStatusCode);
            }

            {
                using var signOutContent = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
                using var signOutResponse = await client.PostAsync("/auth/signout", signOutContent);
                Assert.True(signOutResponse.IsSuccessStatusCode);
            }
        }

        [Fact]
        public async Task RefreshTest()
        {
            using var client = _factory.CreateClient();

            {
                using var loginResponse = await client.GetAsync("/api/login");
                Assert.True(loginResponse.IsSuccessStatusCode);
            }

            {
                using var valuesResponse = await client.GetAsync("/api/values");
                Assert.True(valuesResponse.IsSuccessStatusCode);
            }

            await Task.Delay(31000);

            {
                using var valuesResponse = await client.GetAsync("/api/values");
                Assert.False(valuesResponse.IsSuccessStatusCode);
            }

            {
                using var refreshContent = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
                using var refreshResponse = await client.PostAsync("/auth/session/refresh", refreshContent);
                Assert.True(refreshResponse.IsSuccessStatusCode);
            }

            {
                using var valuesResponse = await client.GetAsync("/api/values");
                Assert.True(valuesResponse.IsSuccessStatusCode);
            }

            {
                using var signOutContent = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
                using var signOutResponse = await client.PostAsync("/auth/signout", signOutContent);
                Assert.True(signOutResponse.IsSuccessStatusCode);
            }
        }

        public class TestWebApplicationFactory : WebApplicationFactory<Startup>
        {
            protected override IHost CreateHost(IHostBuilder builder)
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
                return base.CreateHost(builder);
            }

            protected override IHostBuilder CreateHostBuilder() =>
                Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    });
        }
    }
}
