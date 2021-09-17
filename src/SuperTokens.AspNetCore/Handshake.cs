using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SuperTokens.AspNetCore
{
    public sealed class Handshake
    {
        private ImmutableArray<AccessTokenSigningKey> accessTokenSigningPublicKeyList;

        public Handshake(
            IEnumerable<AccessTokenSigningKey> jwtSigningPublicKeyList,
            bool accessTokenBlacklistingEnabled,
            TimeSpan accessTokenLifetime,
            TimeSpan refreshTokenLifetime)
        {
            this.AccessTokenSigningPublicKeyList = ImmutableArray.ToImmutableArray(jwtSigningPublicKeyList);

            this.AccessTokenBlacklistingEnabled = accessTokenBlacklistingEnabled;
            this.AccessTokenLifetime = accessTokenLifetime;
            this.RefreshTokenLifetime = refreshTokenLifetime;
        }

        public ImmutableArray<AccessTokenSigningKey> AccessTokenSigningPublicKeyList
        {
            get => accessTokenSigningPublicKeyList.Where(keyInfo => keyInfo.Expiration > DateTimeOffset.Now).ToImmutableArray();
            private set => accessTokenSigningPublicKeyList = value;
        }

        public bool AccessTokenBlacklistingEnabled { get; }

        public TimeSpan AccessTokenLifetime { get; }

        public TimeSpan RefreshTokenLifetime { get; }
    }
}
