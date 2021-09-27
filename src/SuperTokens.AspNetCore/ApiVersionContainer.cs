using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperTokens.Net;

namespace SuperTokens.AspNetCore
{
    internal sealed class ApiVersionContainer : BackgroundService, IApiVersionContainer
    {
        private static readonly TimeSpan AutomaticRefreshInterval = new(0, 12, 0, 0, 0);

        private static readonly TimeSpan RetryRefreshInterval = new(0, 0, 5, 0, 0);

        private static readonly string[] SupportedApiVersions = new[] { "2.9", "2.8", "2.7" }.OrderByDescending(str => new Version(str)).ToArray();

        private readonly ISystemClock _clock;

        private readonly ILogger<ApiVersionContainer> _logger;

        private readonly SemaphoreSlim _refreshLock = new(1);

        private readonly IServiceProvider _services;

        private string? _apiVersion;

        private DateTimeOffset _refreshAfter = DateTimeOffset.MinValue;

        public ApiVersionContainer(IServiceProvider services, ISystemClock clock, ILogger<ApiVersionContainer> logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ValueTask<string> GetApiVersionAsync(CancellationToken cancellationToken)
        {
            var apiVersion = _apiVersion;
            return apiVersion != null
                ? ValueTask.FromResult(apiVersion)
                : new ValueTask<string>(this.RefreshApiVersionAsync(cancellationToken));
        }

        public ValueTask<string> GetApiVersionAsync()
        {
            return this.GetApiVersionAsync(CancellationToken.None);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                stoppingToken.ThrowIfCancellationRequested();

                try
                {
                    await this.RefreshApiVersionAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to obtain API versions from SuperTokens Core.");
                }

                var delay = _refreshAfter - _clock.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        private static NotSupportedException NewNotSupportedException(Exception? innerException = null) =>
            new("This version of the SuperTokens SDK does not support the specified SuperTokens Core.", innerException);

        private async Task<string> RefreshApiVersionAsync(CancellationToken cancellationToken)
        {
            var now = _clock.UtcNow;

            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_refreshAfter <= now)
                {
                    var coreApiClient = _services.GetRequiredService<ICoreApiClient>();
                    try
                    {
                        var result = await coreApiClient.GetApiVersionAsync(null, CancellationToken.None);
                        string? matchedApiVersion = null;
                        foreach (var supportedVersion in SupportedApiVersions)
                        {
                            foreach (var version in result.Versions)
                            {
                                if (supportedVersion.Equals(version, StringComparison.Ordinal))
                                {
                                    matchedApiVersion = version;
                                    break;
                                }
                            }

                            if (matchedApiVersion != null)
                            {
                                break;
                            }
                        }

                        _apiVersion = matchedApiVersion;
                        _refreshAfter = now.Add(AutomaticRefreshInterval);
                    }
                    catch (Exception e)
                    {
                        _refreshAfter = now.Add(RetryRefreshInterval);
                        if (_apiVersion == null)
                        {
                            throw NewNotSupportedException(e);
                        }
                        else
                        {
                            _logger.LogError(e, "Failed to obtain updated API versions from SuperTokens Core.");
                        }
                    }
                }

                if (_apiVersion == null)
                {
                    throw NewNotSupportedException();
                }

                return _apiVersion;
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }
}
