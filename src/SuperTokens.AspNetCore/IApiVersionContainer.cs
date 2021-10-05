using System.Threading;
using System.Threading.Tasks;

namespace SuperTokens.AspNetCore
{
    public interface IApiVersionContainer
    {
        ValueTask<string> GetApiVersionAsync(string? apiKey);

        ValueTask<string> GetApiVersionAsync(string? apiKey, CancellationToken cancellationToken);
    }
}
