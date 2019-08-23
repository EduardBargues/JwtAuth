using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Auth
{
    public static class Extensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<AuthMiddleware>();

        public static IServiceCollection AddAuth(this IServiceCollection collection)
            => collection
                .AddSingleton<IRequestManager, RequestPipelineManager>()
                .AddSingleton<IAuthService, AuthService>();
    }
}
