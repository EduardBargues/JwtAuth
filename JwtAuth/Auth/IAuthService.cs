using Microsoft.AspNetCore.Http;

namespace Auth
{
    public interface IAuthService
    {
        (string accessToken, string refreshToken) GenerateAuth();
        AuthStatus RefreshAuth(string accessToken, string refreshToken);
    }
}