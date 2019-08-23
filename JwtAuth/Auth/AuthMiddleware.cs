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
        private readonly IAuthService authService;
        private readonly string accessTokenKey = "access-token";
        private readonly string refreshTokenKey = "refresh-token";
        private readonly string authenticationResponseKey = "authentication-response";

        public AuthMiddleware(RequestDelegate requestDelegate, IAuthService serviceOptions)
        {
            next = requestDelegate;
            authService = serviceOptions;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //(string accessToken, string refreshToken) = authService.GenerateAuth();
            bool okAccessToken = context.Request.Headers.TryGetValue(accessTokenKey, out StringValues accessTokenValues)
                && accessTokenValues.Any();
            bool okRefreshToken = context.Request.Headers.TryGetValue(refreshTokenKey, out StringValues refreshTokenValues)
                && refreshTokenValues.Any();
            if (okAccessToken && okRefreshToken)
            {
                string accessToken = accessTokenValues[0];
                string refreshToken = refreshTokenValues[0];
                AuthStatus status = authService.RefreshAuth(accessToken, refreshToken);
                if (status.Valid)
                {
                    ValidStatus(context, status.AccessToken, status.RefreshToken);
                    await next(context).ConfigureAwait(false);
                }
                else
                    InvalidadStatus(context);
            }
            else
            {
                context.Response.Headers.Add(authenticationResponseKey, "Access or refresh token not provided");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private void ValidStatus(HttpContext context, string accessToken, string refreshToken)
        {
            context.Response.Headers.Add(authenticationResponseKey, "Authentication successful");
            context.Response.Headers.Add(accessTokenKey, accessToken);
            context.Response.Headers.Add(refreshTokenKey, refreshToken);
        }

        private void InvalidadStatus(HttpContext context)
        {
            context.Response.Headers.Add(authenticationResponseKey, "Invalid access and refresh tokens");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
