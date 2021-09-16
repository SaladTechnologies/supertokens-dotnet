using System;

namespace SuperTokens.AspNetCore
{
    public sealed class Handshake
    {
        public Handshake(
            string jwtSigningPublicKey,
            DateTimeOffset jwtSigningPublicKeyExpiration,
            bool accessTokenBlacklistingEnabled,
            TimeSpan accessTokenLifetime,
            TimeSpan refreshTokenLifetime)
        {
            this.JwtSigningPublicKey = jwtSigningPublicKey;
            this.JwtSigningPublicKeyExpiration = jwtSigningPublicKeyExpiration;
            this.AccessTokenBlacklistingEnabled = accessTokenBlacklistingEnabled;
            this.AccessTokenLifetime = accessTokenLifetime;
            this.RefreshTokenLifetime = refreshTokenLifetime;
        }

        public bool AccessTokenBlacklistingEnabled { get; }

        public TimeSpan AccessTokenLifetime { get; }

        public string JwtSigningPublicKey { get; }

        public DateTimeOffset JwtSigningPublicKeyExpiration { get; }

        public TimeSpan RefreshTokenLifetime { get; }
    }
}
