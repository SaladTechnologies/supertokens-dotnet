using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion) =>
            this.GetHandshakeAsync(apiKey, cdiVersion, CancellationToken.None);

        public async Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion, CancellationToken cancellationToken)
        {
            var now = _clock.UtcNow;

            var handshake = _handshake;
            if (handshake != null && handshake.GetAccessTokenSigningPublicKeyList(now).Length > 0)
            {
                return handshake;
            }

            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_handshake == null || _handshake.GetAccessTokenSigningPublicKeyList(now).Length == 0)
                {
                    try
                    {
                        var handshakeResponse = await _coreApiClient.GetHandshakeAsync(apiKey, cdiVersion, CancellationToken.None).ConfigureAwait(false);
                        if (handshakeResponse.Status.Equals("OK", StringComparison.OrdinalIgnoreCase))
                        {
                            var jwtSigningPublicKeyList = handshakeResponse.JwtSigningPublicKeyList != null ?
                                handshakeResponse.JwtSigningPublicKeyList.Select(keyInfo =>
                                    new AccessTokenSigningKey(keyInfo.publicKey,
                                        DateTimeOffset.FromUnixTimeMilliseconds(keyInfo.expirationTime),
                                        DateTimeOffset.FromUnixTimeMilliseconds(keyInfo.createdAt)
                                    )
                                ) : new[] { new AccessTokenSigningKey(handshakeResponse.JwtSigningPublicKey,
                                    DateTimeOffset.FromUnixTimeMilliseconds(handshakeResponse.JwtSigningPublicKeyExpiryTime),
                                    now
                                )};

                            _handshake = new Handshake(
                                jwtSigningPublicKeyList,
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

        public async Task OnHandshakeChanged(IEnumerable<Net.SessionRecipe.KeyInfo>? jwtSigningPublicKeyList, string jwtSigningPublicKey, DateTimeOffset jwtSigningPublicKeyExpiration)
        {
            var handshake = _handshake;
            if (handshake == null)
            {
                return;
            }

            var now = _clock.UtcNow;
            var updatedSigningPublicKeyList = jwtSigningPublicKeyList != null
                ? jwtSigningPublicKeyList.Select(keyInfo =>
                    new AccessTokenSigningKey(keyInfo.publicKey,
                        DateTimeOffset.FromUnixTimeMilliseconds(keyInfo.expirationTime),
                        DateTimeOffset.FromUnixTimeMilliseconds(keyInfo.createdAt)
                    ))
                : new[] {
                    new AccessTokenSigningKey(jwtSigningPublicKey,
                        jwtSigningPublicKeyExpiration,
                        now
                )};

            if (handshake.GetAccessTokenSigningPublicKeyList(now).SequenceEqual(updatedSigningPublicKeyList))
            {
                return;
            }

            await _refreshLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_handshake != null && !handshake.GetAccessTokenSigningPublicKeyList(now).SequenceEqual(updatedSigningPublicKeyList))
                {
                    _handshake = new Handshake(
                        updatedSigningPublicKeyList,
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
