using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

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
            Assert.IsTrue(context.Response.Headers.ContainsKey(Strings.AccessTokenKey));
            Assert.IsTrue(context.Response.Headers.ContainsKey(Strings.AuthenticationResponseKey));
        }
    }
}
