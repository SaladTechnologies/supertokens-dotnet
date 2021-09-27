using System.Threading;
using System.Threading.Tasks;

namespace SuperTokens.AspNetCore
{
    public interface IApiVersionContainer
    {
        ValueTask<string> GetApiVersionAsync(CancellationToken cancellationToken);
        ValueTask<string> GetApiVersionAsync();
    }
}
