using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace Auth
{
    [TestFixture]
    internal class TestRequestManager
    {
        [Test]
        public async Task ProcessContext_NoAccessTokenHeader_ResponseWithMissingTokensHeader()
        {
            // Arrange
            Mock<RequestDelegate> delegateMock = new Mock<RequestDelegate>();
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            RequestPipelineManager service = new RequestPipelineManager(authServiceMock.Object);
            HttpContext context = new DefaultHttpContext();

            // Act
            HttpContext processedContext = service.ProcessRequest(context);

            // Assert
            Assert.IsNotEmpty(processedContext.Response.Headers);
            bool hasRespons = processedContext.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.AccessOrRefreshTokenNotProvided, values[0]);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, processedContext.Response.StatusCode);
        }

        [Test]
        public async Task ProcessContext_NoRefreshTokenHeader_ResponseWithMissingTokenHeader()
        {
            // Arrange
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            RequestPipelineManager service = new RequestPipelineManager(authServiceMock.Object);
            HttpContext context = new DefaultHttpContext();
            context.Request.Headers.Add(Strings.AccessTokenKey, "accesstoken");

            // Act
            HttpContext processedContext = service.ProcessRequest(context);

            // Assert
            Assert.IsNotEmpty(processedContext.Response.Headers);
            bool hasRespons = processedContext.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.AccessOrRefreshTokenNotProvided, values[0]);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, processedContext.Response.StatusCode);
        }

        [Test]
        public async Task ProcessContext_InvalidAuthStatus_InvalidAuthResponseProvided()
        {
            // Arrange
            Mock<IAuthService> authServiceMock = GetInvalidAuthService();
            RequestPipelineManager service = new RequestPipelineManager(authServiceMock.Object);
            HttpContext context = GetRequestWithTokens();

            // Act
            HttpContext processedContext = service.ProcessRequest(context);

            // Assert
            Assert.IsNotEmpty(processedContext.Response.Headers);
            bool hasRespons = processedContext.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.InvalidAccessAndRefreshTokens, values[0]);
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, processedContext.Response.StatusCode);
        }

        [Test]
        public async Task ProcessContext_ValidAuthStatus_InvalidAuthResponseProvided()
        {
            // Arrange
            string accessToken = "accessToken";
            string refreshToken = "refreshToken";
            Mock<IAuthService> authServiceMock = GetValidAuthService(accessToken, refreshToken);
            RequestPipelineManager service = new RequestPipelineManager(authServiceMock.Object);
            HttpContext context = GetRequestWithTokens();

            // Act
            HttpContext processedContext = service.ProcessRequest(context);

            // Assert
            Assert.IsNotEmpty(processedContext.Response.Headers);
            bool hasRespons = processedContext.Response.Headers.TryGetValue(Strings.AuthenticationResponseKey, out StringValues values);
            Assert.IsTrue(hasRespons);
            Assert.IsNotEmpty(values);
            Assert.AreEqual(Strings.AuthenticationSuccesful, values[0]);

            bool hasAccessToken = processedContext.Response.Headers.TryGetValue(Strings.AccessTokenKey, out StringValues accessTokenValues);
            Assert.IsTrue(hasAccessToken);
            Assert.IsNotEmpty(accessToken);
            Assert.AreEqual(accessToken, accessTokenValues[0]);

            bool hasRefreshToken = processedContext.Response.Headers.TryGetValue(Strings.RefreshTokenKey, out StringValues refreshTokenValues);
            Assert.IsTrue(hasRefreshToken);
            Assert.IsNotEmpty(refreshTokenValues);
            Assert.AreEqual(refreshToken, refreshTokenValues[0]);

            Assert.AreEqual((int)HttpStatusCode.Accepted, processedContext.Response.StatusCode);
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
