using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace SuperTokens.AspNetCore.Events
{
    public class MessageReceivedContext : ResultContext<SuperTokensOptions>
    {
        public MessageReceivedContext(HttpContext context, AuthenticationScheme scheme, SuperTokensOptions options) :
            base(context, scheme, options)
        {
        }
    }
}
