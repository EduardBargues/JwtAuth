namespace Auth
{
    public class AuthStatus
    {
        public bool Valid { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
