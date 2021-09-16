using System;

namespace SuperTokens.AspNetCore
{
    internal sealed class AccessToken
    {
        public AccessToken(
            string? antiCsrfToken,
            DateTimeOffset expiryTime,
            string? parentRefreshTokenHash1,
            string refreshTokenHash1,
            string sessionHandle,
            DateTimeOffset timeCreated,
            string userData,
            string userId)
        {
            this.AntiCsrfToken = antiCsrfToken;
            this.ExpiryTime = expiryTime;
            this.ParentRefreshTokenHash1 = parentRefreshTokenHash1;
            this.RefreshTokenHash1 = refreshTokenHash1;
            this.SessionHandle = sessionHandle;
            this.TimeCreated = timeCreated;
            this.UserData = userData;
            this.UserId = userId;
        }

        public string? AntiCsrfToken { get; }

        public DateTimeOffset ExpiryTime { get; }

        public string? ParentRefreshTokenHash1 { get; }

        public string RefreshTokenHash1 { get; }

        public string SessionHandle { get; }

        public DateTimeOffset TimeCreated { get; }

        public string UserData { get; }

        public string UserId { get; }
    }
}
