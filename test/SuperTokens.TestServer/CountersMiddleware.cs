using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SuperTokens.TestServer
{
    public class CountersMiddleware
    {
        private readonly Counters _counters;

        private readonly RequestDelegate _next;

        public CountersMiddleware(RequestDelegate next, Counters counters)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _counters = counters ?? throw new ArgumentNullException(nameof(counters));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path.Equals("/beforeeach"))
            {
                _counters.Reset();
            }
            else if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path.Equals("/auth/session/refresh"))
            {
                _counters.IncrementNoOfTimesRefreshAttemptedDuringTest();
            }

            await _next(context);

            if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path.Equals("/") && context.Response.StatusCode == StatusCodes.Status200OK)
            {
                _counters.IncrementNoOfTimesGetSessionCalledDuringTest();
            }
            else if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path.Equals("/auth/session/refresh") && context.Response.StatusCode == StatusCodes.Status200OK)
            {
                _counters.IncrementNoOfTimesRefreshCalledDuringTest();
            }
        }
    }
}
