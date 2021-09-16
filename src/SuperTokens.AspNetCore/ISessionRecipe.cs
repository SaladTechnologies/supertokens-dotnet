using System.Threading.Tasks;

namespace SuperTokens.AspNetCore
{
    public interface ISessionRecipe
    {
        Task<SuperTokensSession> AuthenticateAsync(string userId);
    }
}
