using System.Net.Http;
using System.Web.Http.Results;
using Microsoft.Owin;
using OwinActionCookieMiddleware;

// ReSharper disable once CheckNamespace
namespace System.Web.Http
{
    public static class ActionCookieApiControllerExtensions
    {
        public static void ChallengeActionMiddleware(this ApiController controller, ActionData actionData)
            => controller.Request.GetOwinContext().ChallengeActionMiddleware(actionData);

        public static IHttpActionResult Action(this ApiController controller, ActionData actionData)
        {
            controller.ChallengeActionMiddleware(actionData);
            return new OkResult(controller);
        }

        public static IHttpActionResult Action(this ApiController controller, string action)
            => controller.Action(new ActionData { Action = action });
    }
}