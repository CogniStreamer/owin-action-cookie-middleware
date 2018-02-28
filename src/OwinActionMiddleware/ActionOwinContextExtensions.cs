using System;
using OwinActionMiddleware;

// ReSharper disable once CheckNamespace
namespace Microsoft.Owin
{
    public static class ActionOwinContextExtensions
    {
        private static readonly string UniqueKey = $"ChallengeActionMiddleware.{Guid.NewGuid():N}";

        public static IOwinContext ChallengeActionMiddleware(this IOwinContext context, ActionData actionData)
            => context.Set(UniqueKey, actionData);

        internal static ActionData GetActionData(this IOwinContext context)
            => context.Get<ActionData>(UniqueKey);
    }
}
