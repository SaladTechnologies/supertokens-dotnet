using System.Threading.Tasks;

namespace SuperTokens.AspNetCore
{
    internal static class TaskUtilities
    {
        public static readonly Task<bool> FalseTask = Task.FromResult(false);

        public static readonly Task<bool> TrueTask = Task.FromResult(true);
    }
}
