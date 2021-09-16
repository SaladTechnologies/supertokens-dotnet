using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using SuperTokens.Net;

namespace SuperTokens.AspNetCore
{
    internal class HandshakeContainer : IHandshakeContainer
    {
        private readonly ISystemClock _clock;

        private readonly ICoreApiClient _coreApiClient;

        private readonly ILogger<HandshakeContainer> _logger;

        private readonly SemaphoreSlim _refreshLock = new(1);

        private Handshake? _handshake;

        public HandshakeContainer(ICoreApiClient coreApiClient, ILogger<HandshakeContainer> logger, ISystemClock clock)
        {
            _coreApiClient = coreApiClient ?? throw new ArgumentNullException(nameof(coreApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public async Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion, CancellationToken cancellationToken)
        {
            var now = _clock.UtcNow;

            var handshake = _handshake;
            if (handshake != null && handshake.JwtSigningPublicKeyExpiration > now)
            {
                return handshake;
            }

            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_handshake == null || _handshake.JwtSigningPublicKeyExpiration <= now)
                {
                    try
                    {
                        var handshakeResponse = await _coreApiClient.GetHandshakeAsync(apiKey, cdiVersion, CancellationToken.None).ConfigureAwait(false);
                        if (handshakeResponse.Status.Equals("OK", StringComparison.OrdinalIgnoreCase))
                        {
                            _handshake = new Handshake(
                                handshakeResponse.JwtSigningPublicKey,
                                DateTimeOffset.FromUnixTimeMilliseconds(handshakeResponse.JwtSigningPublicKeyExpiryTime),
                                handshakeResponse.AccessTokenBlacklistingEnabled,
                                TimeSpan.FromMilliseconds(handshakeResponse.AccessTokenValidity),
                                TimeSpan.FromMilliseconds(handshakeResponse.RefreshTokenValidity));
                        }
                    }
                    catch (CoreApiResponseException e)
                    {
                        _logger.LogError(e, "Failed to get SuperTokens handshake");
                    }
                    catch (HttpRequestException e)
                    {
                        _logger.LogError(e, "Failed to get SuperTokens handshake");
                    }
                    catch (JsonException e)
                    {
                        _logger.LogError(e, "Failed to parse SuperTokens handshake");
                    }
                }

                if (_handshake != null)
                {
                    return _handshake;
                }
                else
                {
                    throw new InvalidOperationException("No SuperTokens handshake is available");
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public async Task OnHandshakeChanged(string jwtSigningPublicKey, DateTimeOffset jwtSigningPublicKeyExpiration)
        {
            var handshake = _handshake;
            if (handshake == null)
            {
                return;
            }

            if (handshake.JwtSigningPublicKey.Equals(jwtSigningPublicKey, StringComparison.Ordinal) &&
                handshake.JwtSigningPublicKeyExpiration.ToUnixTimeSeconds() == jwtSigningPublicKeyExpiration.ToUnixTimeSeconds())
            {
                return;
            }

            await _refreshLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_handshake != null && (!_handshake.JwtSigningPublicKey.Equals(jwtSigningPublicKey, StringComparison.Ordinal) || _handshake.JwtSigningPublicKeyExpiration.ToUnixTimeSeconds() != jwtSigningPublicKeyExpiration.ToUnixTimeSeconds()))
                {
                    _handshake = new Handshake(
                        jwtSigningPublicKey,
                        jwtSigningPublicKeyExpiration,
                        _handshake.AccessTokenBlacklistingEnabled,
                        _handshake.AccessTokenLifetime,
                        _handshake.RefreshTokenLifetime);
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }
}
