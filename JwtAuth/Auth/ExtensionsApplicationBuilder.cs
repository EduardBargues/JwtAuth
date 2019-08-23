using Microsoft.AspNetCore.Builder;

namespace Auth
{
    public static class ExtensionsApplicationBuilder
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<AuthMiddleware>();
    }
}
