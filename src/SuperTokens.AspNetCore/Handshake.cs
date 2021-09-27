using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SuperTokens.AspNetCore
{
    public sealed class Handshake
    {
        private ImmutableArray<AccessTokenSigningKey> _accessTokenSigningPublicKeyList;

        public Handshake(
            IEnumerable<AccessTokenSigningKey> jwtSigningPublicKeyList,
            bool accessTokenBlacklistingEnabled,
            TimeSpan accessTokenLifetime,
            TimeSpan refreshTokenLifetime)
        {
            _accessTokenSigningPublicKeyList = ImmutableArray.ToImmutableArray(jwtSigningPublicKeyList);

            this.AccessTokenBlacklistingEnabled = accessTokenBlacklistingEnabled;
            this.AccessTokenLifetime = accessTokenLifetime;
            this.RefreshTokenLifetime = refreshTokenLifetime;
        }

        public ImmutableArray<AccessTokenSigningKey> GetAccessTokenSigningPublicKeyList(DateTimeOffset now) =>
            _accessTokenSigningPublicKeyList.Where(keyInfo => keyInfo.Expiration > now).ToImmutableArray();

        public bool AccessTokenBlacklistingEnabled { get; }

        public TimeSpan AccessTokenLifetime { get; }

        public TimeSpan RefreshTokenLifetime { get; }
    }
}
