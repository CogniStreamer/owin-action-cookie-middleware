using OwinActionCookieMiddleware;

// ReSharper disable once CheckNamespace
namespace Owin
{
    public static class ActionCookieIAppBuilderExtensions
    {
        public static IAppBuilder UseActionCookieMiddleware(this IAppBuilder app, ActionCookieMiddlewareOptions options)
            => app.Use<ActionCookieMiddleware>(options);
    }
}
