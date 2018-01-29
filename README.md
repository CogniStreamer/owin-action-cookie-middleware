# OWIN Action Cookie Middleware

[![Build status](https://ci.appveyor.com/api/projects/status/wm6ior5dbbg2705u/branch/master?svg=true)](https://ci.appveyor.com/project/CogniStreamer/owin-action-cookie-middleware/branch/master)

OWIN middleware that allows sending actions to a single page application in the form of a cookie.

When the middleware is challenged, it will return an JavaScript readable cookie containing an action object as JSON.

This is useful in cases where the back-end requests a single-page application to perform a certain action without having to hardcode front-end routes.

F.e. when sending out notification e-mails, you sometimes want to include back-end links in the e-mail.

## Get it on [NuGet](https://www.nuget.org/packages/OwinActionCookieMiddleware/)

    PM> Install-Package OwinActionCookieMiddleware
    PM> Install-Package OwinActionCookieMiddleware.WebApi

## Usage

The base package contains an extension method to be used on the `IOwinContext` interface, while the WebApi package adds an extension method to be used in an `ApiController`.

### From OWIN middleware

    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Path != new PathString("/test")) await next();
        ctx.ChallengeActionMiddleware(new ActionData { Action = "JumpAround" });
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
    });

### From WebApi controller

    [RoutePrefix("navigate")]
    public class MyApiController : ApiController
    {
        [HttpGet, Route("userprofile")]
        public IHttpActionResult Action(Guid userId)
        {
            return this.Action(new OpenUserProfileAction(userId));
        }
    }

    public class OpenUserProfileAction : Action
    {
        public OpenUserProfileAction(Guid userId)
        {
            Action = "OpenUserProfile";
            UserId = userId;
        }

        public Guid UserId { get; }
    }
