using System;
using OwinActionCookieMiddleware;

// ReSharper disable once CheckNamespace
namespace Microsoft.Owin
{
    public static class ActionCookieOwinContextExtensions
    {
        private static readonly string UniqueKey = $"ChallengeActionCookie.{Guid.NewGuid():N}";

        public static IOwinContext ChallengeActionMiddleware(this IOwinContext context, ActionData actionData)
            => context.Set(UniqueKey, actionData);

        internal static ActionData GetActionData(this IOwinContext context)
            => context.Get<ActionData>(UniqueKey);
    }
}
