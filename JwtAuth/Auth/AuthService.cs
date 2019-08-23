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
        private readonly SymmetricSecurityKey securityKey;
        private readonly JwtSecurityTokenHandler tokenHandler;
        private readonly TokenValidationParameters validationParameters;

        public AuthService(IOptions<AuthConfiguration> configuration)
        {
            this.configuration = configuration.Value;
            securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.configuration.Seed));
            tokenHandler = new JwtSecurityTokenHandler();
            validationParameters = new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateAudience = false,
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false,
                ValidateTokenReplay = false,
                IssuerSigningKey = securityKey
            };
        }

        public AuthStatus RefreshAuth(string accessToken, string refreshToken)
        {
            bool accessTokenValid = IsTokenValid(accessToken);
            bool refreshTokenValid = IsTokenValid(refreshToken);
            AuthStatus status = new AuthStatus() { Valid = refreshTokenValid };
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
            SecurityToken token = tokenHandler.CreateToken(
                new SecurityTokenDescriptor
                {
                    Expires = DateTime.UtcNow.Add(expiration),
                    SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
                });
            return tokenHandler.WriteToken(token);
        }

        private bool IsTokenValid(string token)
        {
            bool isValid;
            try
            {
                IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                isValid = true;
            }
            catch (Exception e)
            {
                isValid = false;
            }
            return isValid;
        }
    }
}