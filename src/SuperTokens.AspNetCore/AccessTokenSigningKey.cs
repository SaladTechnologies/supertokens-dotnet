using System;

namespace SuperTokens.AspNetCore
{
    public sealed class AccessTokenSigningKey : IEquatable<AccessTokenSigningKey?>
    {
        public AccessTokenSigningKey(
            string publicKey,
            DateTimeOffset expiration,
            DateTimeOffset creation)
        {
            this.PublicKey = publicKey;
            this.Expiration = expiration;
            this.Creation = creation;
        }

        public string PublicKey { get; }

        public DateTimeOffset Expiration { get; }
        public DateTimeOffset Creation { get; }

        public override bool Equals(object? obj) => this.Equals(obj as AccessTokenSigningKey);

        // We do not consider the creation time during equality check: this information is not available on old APIs.
        public bool Equals(AccessTokenSigningKey? other) => other != null && this.PublicKey == other.PublicKey && this.Expiration.Equals(other.Expiration);
        public override int GetHashCode() => HashCode.Combine(this.PublicKey, this.Expiration, this.Creation);
    }
}
