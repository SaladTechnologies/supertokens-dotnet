namespace SuperTokens.AspNetCore
{
    /// <summary>Provides access to the current <see cref="SuperTokensSession"/>, if one is available.</summary>
    public interface ISessionAccessor
    {
        /// <summary>
        ///     Gets or sets the current <see cref="SuperTokensSession"/>. Returns <c>null</c> if there is no active
        ///     <see cref="SuperTokensSession"/>.
        /// </summary>
        public SuperTokensSession? Session { get; set; }
    }
}
