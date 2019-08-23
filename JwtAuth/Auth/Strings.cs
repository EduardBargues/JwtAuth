using Microsoft.Extensions.Primitives;

namespace Auth
{
    internal static class Strings
    {
        public static string AccessTokenKey => "access-token";
        public static string RefreshTokenKey => "refresh-token";
        public static string AuthenticationResponseKey => "authentication-response";
        public static string AccessOrRefreshTokenNotProvided => "Access or refresh token not provided";
        public static string InvalidAccessAndRefreshTokens => "Invalid access and refresh tokens";
        public static string AuthenticationSuccesful => "Authentication successful";
    }
}
