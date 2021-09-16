namespace SuperTokens.AspNetCore
{
    /// <summary>Provides an implementation of <see cref="ISessionAccessor"/>.</summary>
    internal class SessionAccessor : ISessionAccessor
    {
        /// <inheritdoc/>
        public SuperTokensSession? Session { get; set; }
    }
}
