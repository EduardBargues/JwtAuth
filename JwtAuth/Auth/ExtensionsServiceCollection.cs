using Microsoft.Extensions.DependencyInjection;

namespace Auth
{
    public static class ExtensionsServiceCollection
    {
        public static IServiceCollection AddAuth(this IServiceCollection collection)
            => collection.AddSingleton<IAuthService, AuthService>();
    }
}
