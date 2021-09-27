using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperTokens.AspNetCore
{
    public interface IHandshakeContainer
    {
        Task<Handshake> GetHandshakeAsync(string? apiKey, string? cdiVersion, CancellationToken cancellationToken);

        Task OnHandshakeChanged(IEnumerable<Net.SessionRecipe.KeyInfo>? jwtSigningPublicKeyList, string jwtSigningPublicKey, DateTimeOffset jwtSigningPublicKeyExpiration);
    }
}
