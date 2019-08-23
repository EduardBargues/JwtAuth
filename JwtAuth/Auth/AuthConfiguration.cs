using System;

namespace Auth
{
    public class AuthConfiguration
    {
        public string Seed { get; set; }
        public TimeSpan AccessTokenExpirationSpan { get; set; }
        public TimeSpan RefreshTokenExpirationSpan { get; set; }
    }
}