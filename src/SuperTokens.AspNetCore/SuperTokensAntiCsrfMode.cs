namespace SuperTokens.AspNetCore
{
    public enum SuperTokensAntiCsrfMode
    {
        None = 0,

        ViaCustomHeader,

        ViaToken,
    }
}
