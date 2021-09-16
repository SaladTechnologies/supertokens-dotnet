using System;
using System.Threading.Tasks;

namespace SuperTokens.AspNetCore.Events
{
    public class SuperTokensEvents
    {
        public Func<MessageReceivedContext, Task> OnMessageReceived { get; set; } = context => Task.CompletedTask;

        public virtual Task MessageReceived(MessageReceivedContext context) =>
            this.OnMessageReceived(context);
    }
}
