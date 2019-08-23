using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Auth
{
    internal class AuthService : IAuthService
    {
        private readonly AuthConfiguration configuration;

        public AuthService(IOptions<AuthConfiguration> configuration)
            => this.configuration = configuration.Value;

        public AuthStatus RefreshAuth(string accessToken, string refreshToken)
        {
            bool accessTokenValid = IsTokenValid(accessToken);
            bool refreshTokenValid = IsTokenValid(refreshToken);
            AuthStatus status = new AuthStatus() { Valid = accessTokenValid && refreshTokenValid };
            if (!accessTokenValid && refreshTokenValid)
            {
                status.AccessToken = GenerateToken(configuration.AccessTokenExpirationSpan);
                status.RefreshToken = GenerateToken(configuration.RefreshTokenExpirationSpan);
            }
            if (accessTokenValid)
            {
                status.AccessToken = accessToken;
                status.RefreshToken = refreshToken;
            }

            return status;
        }

        public (string accessToken, string refreshToken) GenerateAuth()
            => (accessToken: GenerateToken(configuration.AccessTokenExpirationSpan),
                refreshToken: GenerateToken(configuration.RefreshTokenExpirationSpan));

        private string GenerateToken(TimeSpan expiration)
        {
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.Add(expiration),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.Seed)),
                    SecurityAlgorithms.HmacSha256Signature),
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool IsTokenValid(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.Seed))
            };
            bool isValid;
            try
            {
                IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                isValid = true;
            }
            catch (Exception)
            {
                isValid = false;
            }
            return isValid;
        }
    }
}