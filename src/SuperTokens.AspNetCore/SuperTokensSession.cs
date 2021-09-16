namespace SuperTokens.AspNetCore
{
    public sealed class SuperTokensSession
    {
        public SuperTokensSession(string handle, string userId, string userDataInJwt)
        {
            this.Handle = handle;
            this.UserId = userId;
            this.UserDataInJwt = userDataInJwt;
        }

        public string Handle { get; }

        public string UserId { get; }

        public string UserDataInJwt { get; }
    }
}
