using System;
using System.Threading;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;

using Moq;

using NUnit.Framework;

namespace Auth
{
    [TestFixture]
    public class TestAuthService
    {
        [Test]
        public void GenerateAuth_StandardConfiguration_ProvidedTokensValid()
        {
            // Arrange
            IdentityModelEventSource.ShowPII = true;
            Mock<IOptions<AuthConfiguration>> mock = GetAuthConfiguration(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(10000));
            IAuthService service = new AuthService(mock.Object);

            // Act
            (string accessToken, string refreshToken) = service.GenerateAuth();

            // Assert
            Assert.IsNotNull(accessToken);
            Assert.IsNotEmpty(accessToken);
            Assert.IsNotNull(refreshToken);
            Assert.IsNotEmpty(refreshToken);
        }

        [Test]
        public void RefreshAuth_ValidAccessToken_SameTokensProvided()
        {
            // Arrange
            IdentityModelEventSource.ShowPII = true;
            Mock<IOptions<AuthConfiguration>> mock = GetAuthConfiguration(TimeSpan.FromMilliseconds(10000), TimeSpan.FromMilliseconds(100000));
            IAuthService service = new AuthService(mock.Object);
            (string accessToken, string refreshToken) = service.GenerateAuth();

            // Act
            AuthStatus status = service.RefreshAuth(accessToken, refreshToken);

            // Assert
            Assert.IsTrue(status.Valid);
            Assert.AreEqual(accessToken, status.AccessToken);
            Assert.AreEqual(refreshToken, status.RefreshToken);
        }

        [Test]
        public void RefreshAuth_InValidAccessTokenValidRefreshToken_NewTokensProvided()
        {
            // Arrange
            IdentityModelEventSource.ShowPII = true;
            TimeSpan accessTokenExpiration = TimeSpan.FromMilliseconds(1000);
            Mock<IOptions<AuthConfiguration>> mock = GetAuthConfiguration(accessTokenExpiration, TimeSpan.FromMilliseconds(2000));
            IAuthService service = new AuthService(mock.Object);
            (string accessToken, string refreshToken) = service.GenerateAuth();
            Thread.Sleep(accessTokenExpiration);

            // Act
            AuthStatus status = service.RefreshAuth(accessToken, refreshToken);

            // Assert
            Assert.IsTrue(status.Valid);
            Assert.IsNotNull(status.AccessToken);
            Assert.IsNotEmpty(status.AccessToken);
            Assert.IsNotNull(status.RefreshToken);
            Assert.IsNotEmpty(status.RefreshToken);
            Assert.AreNotEqual(accessToken, status.AccessToken);
            Assert.AreNotEqual(refreshToken, status.RefreshToken);
        }

        [Test]
        public void RefreshAuth_InValidTokens_InvalidStatusProvided()
        {
            // Arrange
            IdentityModelEventSource.ShowPII = true;
            TimeSpan accessTokenExpiration = TimeSpan.FromMilliseconds(1000);
            Mock<IOptions<AuthConfiguration>> mock = GetAuthConfiguration(accessTokenExpiration, TimeSpan.FromMilliseconds(1000));
            IAuthService service = new AuthService(mock.Object);
            (string accessToken, string refreshToken) = service.GenerateAuth();
            Thread.Sleep(accessTokenExpiration);

            // Act
            AuthStatus status = service.RefreshAuth(accessToken, refreshToken);

            // Assert
            Assert.IsFalse(status.Valid);
            Assert.IsNull(status.AccessToken);
            Assert.IsNull(status.RefreshToken);
        }

        private Mock<IOptions<AuthConfiguration>> GetAuthConfiguration(TimeSpan accessTokenExpiration, TimeSpan refreshTokenExpiration)
        {
            Mock<IOptions<AuthConfiguration>> mock = new Mock<IOptions<AuthConfiguration>>();
            mock
                .Setup(m => m.Value)
                .Returns(new AuthConfiguration()
                {
                    Seed = Get1024RandomText(),
                    AccessTokenExpirationSpan = accessTokenExpiration,
                    RefreshTokenExpirationSpan = refreshTokenExpiration,
                });
            return mock;
        }

        private string Get1024RandomText()
            => "XjZSAN8mMySi0t3vSCgP5h14R19qbDwbxwIXxtrWLgPTXCyncSGgsVaIqjRQAd6TmTotBlNO6UOY3F5yyR3yaGaDZ59Ll4FC9M1aWtsWezugM3H6bROGLPMDHTmiMIaw9qWr8N9B3KHM6ULCL3GfBpMhZvqikE43mKpAfbz6itwd4GHYdctujvCvWcR2IGgMzwChhNaLZ2nMuOokYhClNKrWa6yTY7kdHLpTw379qzMuIA7WED1vSuSdZLADLqvZHDKe6NKbGsKDbFFRsNdS7i8HaGK69e3zU3wSu3W5gllufBxtu05m5zj4s6djCWzICoR57PH8sYwj2HVd4uFkj5Sz0TPK7kuou0XbijeJ8qYR7stxDiUrdNCt8IsWtIGLfQhDW18wolT3ck01fP0WKaSerlNkL2IhGnOsHTIO801zLNzmlVsIcPvfDv0F30K7Uz5FQFYeaB8BUPA0WwQGsJMzKWKa9Y4zbfZ9AwIiRyVLqO3uH9xoitl9mFKKbCGzktQF1OPcV9SIQoTtGhl61tdoZcWqS5eAfvMIoZT1obbcQqD0T5wz4TeTiGZsp8ISm2hmo9jz92sDwqbDVFWe6jS9qj4dGI0n4iTWLWDteLe9UeQ07VJPehvuuUq6VWrL1HAN3RrX72rna9qqbdDgBaIIjZivQ9MNPZYYBCR0fWzvVnXbYSCSCoiTGaFSbDEG472QSip4MFOTxLtMhGZCVUXEM5D7Y6PfIpp12K5TkGJFT2rmSntyGd3W6O1hBzriMaOmtJkhGwmMVhFjofZOQUyFvBTm25dzmYmUOgIPlIqDGtLi2ckVwW52rVRHskUs2nixuDQkm5In7W7J9kwNSiQSWinENhp3CFk6YsLfyBLnJFCPaZH5TxJRGVE6K7oX0jLAYy3y1zRU0w0d6cgYSaMS1ynXP8wui8ei1aqeEuKXU2MK4TsHjOlDV67K82rMU2bjNJfZog21WiOVi4oZ301fuYJvbqBjaHY8gGxrXM7iswt5QRjcIfg1nP73WPuW";
    }
}
