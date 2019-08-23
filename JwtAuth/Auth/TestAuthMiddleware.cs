using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

using NUnit.Framework;

namespace Auth
{
    [TestFixture]
    public class TestAuthMiddleware
    {
        [Test]
        public async Task InvokeAsync_NoAccessTokenHeader_ResponseWithMissingTokensHeader()
        {
            // Arrange
            Mock<RequestDelegate> delegateMock = new Mock<RequestDelegate>();
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            AuthMiddleware middleware = new AuthMiddleware(delegateMock.Object, authServiceMock.Object);
            HttpContext context = new DefaultHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.IsNotEmpty(context.Response.Headers);
            bool hasRespons = context.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.AccessOrRefreshTokenNotProvided, values[0]);
        }

        [Test]
        public async Task InvokeAsync_NoRefreshTokenHeader_ResponseWithMissingTokenHeader()
        {
            // Arrange
            Mock<RequestDelegate> delegateMock = new Mock<RequestDelegate>();
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            AuthMiddleware middleware = new AuthMiddleware(delegateMock.Object, authServiceMock.Object);
            HttpContext context = new DefaultHttpContext();
            context.Request.Headers.Add(Strings.AccessTokenKey, "accesstoken");

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.IsNotEmpty(context.Response.Headers);
            bool hasRespons = context.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.AccessOrRefreshTokenNotProvided, values[0]);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        }

        [Test]
        public async Task InvokeAsync_InvalidAuthStatus_InvalidAuthResponseProvided()
        {
            // Arrange
            Mock<RequestDelegate> delegateMock = new Mock<RequestDelegate>();
            Mock<IAuthService> authServiceMock = GetInvalidAuthService();
            AuthMiddleware middleware = new AuthMiddleware(delegateMock.Object, authServiceMock.Object);
            HttpContext context = GetRequestWithTokens();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.IsNotEmpty(context.Response.Headers);
            bool hasRespons = context.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.InvalidAccessAndRefreshTokens, values[0]);
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }

        [Test]
        public async Task InvokeAsync_ValidAuthStatus_InvalidAuthResponseProvided()
        {
            // Arrange
            Mock<RequestDelegate> delegateMock = new Mock<RequestDelegate>();
            string accessToken = "accessToken";
            string refreshToken = "refreshToken";
            Mock<IAuthService> authServiceMock = GetValidAuthService(accessToken, refreshToken);
            AuthMiddleware middleware = new AuthMiddleware(delegateMock.Object, authServiceMock.Object);
            HttpContext context = GetRequestWithTokens();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.IsNotEmpty(context.Response.Headers);
            bool hasRespons = context.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.AuthenticationSuccesful, values[0]);

            bool hasAccessToken = context.Response.Headers.TryGetValue(Strings.AccessTokenKey, out StringValues accessTokenValues);
            Assert.IsTrue(hasAccessToken);
            Assert.IsNotEmpty(accessToken);
            Assert.AreEqual(accessToken, accessTokenValues[0]);

            bool hasRefreshToken = context.Response.Headers.TryGetValue(Strings.RefreshTokenKey, out StringValues refreshTokenValues);
            Assert.IsTrue(hasRefreshToken);
            Assert.IsNotEmpty(refreshTokenValues);
            Assert.AreEqual(refreshToken, refreshTokenValues[0]);
        }

        private static Mock<IAuthService> GetValidAuthService(string accessToken, string refreshToken)
        {
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            authServiceMock
                .Setup(m => m.RefreshAuth(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AuthStatus() { Valid = true, AccessToken = accessToken, RefreshToken = refreshToken });
            return authServiceMock;
        }

        private static Mock<IAuthService> GetInvalidAuthService()
        {
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            authServiceMock
                .Setup(m => m.RefreshAuth(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new AuthStatus() { Valid = false, });
            return authServiceMock;
        }

        private static HttpContext GetRequestWithTokens()
        {
            HttpContext context = new DefaultHttpContext();
            context.Request.Headers.Add(Strings.AccessTokenKey, "accesstoken");
            context.Request.Headers.Add(Strings.RefreshTokenKey, "refreshtoken");
            return context;
        }
    }
}
