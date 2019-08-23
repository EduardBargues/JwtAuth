using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Auth
{
    internal class RequestPipelineManager : IRequestManager
    {
        readonly IAuthService authService;

        public RequestPipelineManager(IAuthService authService)
        {
            this.authService = authService;
        }

        public HttpContext ProcessRequest(HttpContext context)
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
                    SetValidStatus(context, status.AccessToken, status.RefreshToken);
                else
                    SetInvalidadStatus(context);
            }
            else
                SetBadRequestStatus(context);

            return context;
        }

        private static void SetBadRequestStatus(HttpContext context)
        {
            context.Response.Headers.Add(Strings.AuthenticationResponseKey, Strings.AccessOrRefreshTokenNotProvided);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        private void SetValidStatus(HttpContext context, string accessToken, string refreshToken)
        {
            context.Response.Headers.Add(Strings.AuthenticationResponseKey, Strings.AuthenticationSuccesful);
            context.Response.Headers.Add(Strings.AccessTokenKey, accessToken);
            context.Response.Headers.Add(Strings.RefreshTokenKey, refreshToken);
            context.Response.StatusCode = (int)HttpStatusCode.Accepted;
        }

        private void SetInvalidadStatus(HttpContext context)
        {
            context.Response.Headers.Add(Strings.AuthenticationResponseKey, Strings.InvalidAccessAndRefreshTokens);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
