using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Auth
{
    internal class AuthMiddleware
    {
        private readonly RequestDelegate next;

        public AuthMiddleware(RequestDelegate requestDelegate)
            => next = requestDelegate;

        public async Task InvokeAsync(HttpContext context)
        {
            IRequestManager service = (IRequestManager)context.Request.HttpContext.RequestServices.GetService(typeof(IRequestManager));
            HttpContext processedContext = service.ProcessRequest(context);

            // This can not be tested due to HttpContext type problems wth moq.
            if (context.Response.StatusCode == (int)HttpStatusCode.Accepted)
                await next(processedContext);
        }
    }
}
