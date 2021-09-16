using System.Threading;
using System.Threading.Tasks;
using SuperTokens.Net.Core;
using SuperTokens.Net.SessionRecipe;

namespace SuperTokens.Net
{
    public interface ICoreApiClient
    {
        Task<CreateSessionResponse> CreateSessionAsync(string? apiKey, string? cdiVersion, CreateSessionRequest body);

        Task<CreateSessionResponse> CreateSessionAsync(string? apiKey, string? cdiVersion, CreateSessionRequest body, CancellationToken cancellationToken);

        Task<DeleteSessionResponse> DeleteSessionAsync(string? apiKey, string? cdiVersion, DeleteSessionRequest body);

        Task<DeleteSessionResponse> DeleteSessionAsync(string? apiKey, string? cdiVersion, DeleteSessionRequest body, CancellationToken cancellationToken);

        Task<ApiVersionResponse> GetApiVersionAsync(string? apiKey);

        Task<ApiVersionResponse> GetApiVersionAsync(string? apiKey, CancellationToken cancellationToken);

        Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion);

        Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion, CancellationToken cancellationToken);

        Task<SessionResponse> GetSessionAsync(string? apiKey, string? cdiVersion, string sessionHandle);

        Task<SessionResponse> GetSessionAsync(string? apiKey, string? cdiVersion, string sessionHandle, CancellationToken cancellationToken);

        Task<SessionHandlesResponse> GetSessionHandlesAsync(string? apiKey, string? cdiVersion, string userId);

        Task<SessionHandlesResponse> GetSessionHandlesAsync(string? apiKey, string? cdiVersion, string userId, CancellationToken cancellationToken);

        Task<RefreshSessionResponse> RefreshSessionAsync(string? apiKey, string? cdiVersion, RefreshSessionRequest body);

        Task<RefreshSessionResponse> RefreshSessionAsync(string? apiKey, string? cdiVersion, RefreshSessionRequest body, CancellationToken cancellationToken);

        Task<RegenerateSessionResponse> RegenerateSessionAsync(string? apiKey, string? cdiVersion, RegenerateSessionRequest body);

        Task<RegenerateSessionResponse> RegenerateSessionAsync(string? apiKey, string? cdiVersion, RegenerateSessionRequest body, CancellationToken cancellationToken);

        Task<VerifySessionResponse> VerifySessionAsync(string? apiKey, string? cdiVersion, VerifySessionRequest body);

        Task<VerifySessionResponse> VerifySessionAsync(string? apiKey, string? cdiVersion, VerifySessionRequest body, CancellationToken cancellationToken);
    }
}
