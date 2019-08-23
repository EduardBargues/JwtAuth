using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auth
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class AuthAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {

        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            IRequestManager service = (IRequestManager)context.HttpContext.RequestServices.GetService(typeof(IRequestManager));
            HttpContext processedContext = service.ProcessRequest(context.HttpContext);

            HttpStatusCode status = (HttpStatusCode)processedContext.Response.StatusCode;
            if (status != HttpStatusCode.Accepted)
                context.Result = new ContentResult()
                {
                    StatusCode = (int)status,
                    Content = status.ToString(),
                    ContentType = "text"
                };
        }
    }
}
