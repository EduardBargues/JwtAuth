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

        public AuthMiddleware(RequestDelegate requestDelegate, IAuthService serviceOptions)
        {
            next = requestDelegate;
            authService = serviceOptions;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            bool okAccessToken = context.Request.Headers.TryGetValue(Strings.AccessTokenKey, out StringValues accessTokenValues)
                && accessTokenValues.Any();
            bool okRefreshToken = context.Request.Headers.TryGetValue(Strings.RefreshTokenKey, out StringValues refreshTokenValues)
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
                context.Response.Headers.Add(Strings.AuthenticationResponseKey, Strings.AccessOrRefreshTokenNotProvided);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private void ValidStatus(HttpContext context, string accessToken, string refreshToken)
        {
            context.Response.Headers.Add(Strings.AuthenticationResponseKey, Strings.AuthenticationSuccesful);
            context.Response.Headers.Add(Strings.AccessTokenKey, accessToken);
            context.Response.Headers.Add(Strings.RefreshTokenKey, refreshToken);
        }

        private void InvalidadStatus(HttpContext context)
        {
            context.Response.Headers.Add(Strings.AuthenticationResponseKey, Strings.InvalidAccessAndRefreshTokens);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
