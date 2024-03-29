﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperTokens.Net;

namespace SuperTokens.AspNetCore
{
    internal sealed class ApiVersionContainer : BackgroundService, IApiVersionContainer
    {
        private static readonly TimeSpan AutomaticRefreshInterval = new(0, 12, 0, 0, 0);

        private static readonly TimeSpan MinAutomaticRefreshInterval = new(0, 0, 1, 0, 0);

        private static readonly TimeSpan RetryRefreshInterval = new(0, 0, 5, 0, 0);

        private static readonly string[] SupportedApiVersions = new[] { "2.9", "2.8", "2.7" }.OrderByDescending(str => new Version(str)).ToArray();

        private readonly ISystemClock _clock;

        private readonly ILogger<ApiVersionContainer> _logger;

        private readonly IOptionsMonitor<SuperTokensOptions> _options;

        private readonly SemaphoreSlim _refreshLock = new(1);

        private readonly IServiceProvider _services;

        private string? _apiVersion;

        private DateTimeOffset _refreshAfter = DateTimeOffset.MinValue;

        public ApiVersionContainer(IServiceProvider services, ISystemClock clock, IOptionsMonitor<SuperTokensOptions> options, ILogger<ApiVersionContainer> logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ValueTask<string> GetApiVersionAsync(string? apiKey) =>
            this.GetApiVersionAsync(apiKey, CancellationToken.None);

        public ValueTask<string> GetApiVersionAsync(string? apiKey, CancellationToken cancellationToken)
        {
            var apiVersion = _apiVersion;
            return apiVersion != null
                ? ValueTask.FromResult(apiVersion)
                : new ValueTask<string>(this.RefreshApiVersionAsync(apiKey, cancellationToken));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                stoppingToken.ThrowIfCancellationRequested();

                try
                {
                    var options = _options.Get(SuperTokensDefaults.AuthenticationScheme);
                    await this.RefreshApiVersionAsync(options.CoreApiKey, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to obtain API versions from SuperTokens Core.");
                }

                var delay = _refreshAfter - _clock.UtcNow;
                if (delay < MinAutomaticRefreshInterval)
                {
                    delay = MinAutomaticRefreshInterval;
                }

                await Task.Delay(delay, stoppingToken);
            }
        }

        private static NotSupportedException NewNotSupportedException(Exception? innerException = null) =>
            new("This version of the SuperTokens SDK does not support the specified SuperTokens Core.", innerException);

        private async Task<string> RefreshApiVersionAsync(string? apiKey, CancellationToken cancellationToken)
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
                        var result = await coreApiClient.GetApiVersionAsync(apiKey, CancellationToken.None);
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
