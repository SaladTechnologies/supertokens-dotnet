using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperTokens.AspNetCore
{
    public interface IHandshakeContainer
    {
        Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion, CancellationToken cancellationToken);

        Task OnHandshakeChanged(string jwtSigningPublicKey, DateTimeOffset jwtSigningPublicKeyExpiration);
    }
}
