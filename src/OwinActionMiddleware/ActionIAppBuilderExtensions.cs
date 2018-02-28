using OwinActionMiddleware;

// ReSharper disable once CheckNamespace
namespace Owin
{
    public static class ActionIAppBuilderExtensions
    {
        public static IAppBuilder UseActionMiddleware(this IAppBuilder app, ActionMiddlewareOptions options)
            => app.Use<ActionMiddleware>(options);
    }
}
