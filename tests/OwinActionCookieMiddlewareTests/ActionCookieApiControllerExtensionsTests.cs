using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using OwinActionCookieMiddleware;

namespace OwinActionCookieMiddlewareTests
{
    [TestFixture]
    public class ActionCookieApiControllerExtensionsTests
    {
        [Test]
        public async Task ChallengeActionMiddleware_ShouldAddDataToOwinEnvironment()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseActionCookieMiddleware(new ActionCookieMiddlewareOptions
                {
                    ApplicationUrl = new Uri("https://test.server.com"),
                    CookieName = "ACT"
                });
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
                app.UseWebApi(config);
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test/challenge");
                var cookie = response.Headers.GetValues("Set-Cookie").First();
                Assert.That(cookie, Is.EqualTo("ACT=%7B%22action%22%3A%22FromChallenge%22%7D; domain=.localhost; path=/"));
            }
        }

        [Test]
        public async Task Action_ShouldAddDataToOwinEnvironment()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseActionCookieMiddleware(new ActionCookieMiddlewareOptions
                {
                    ApplicationUrl = new Uri("https://test.server.com"),
                    CookieName = "ACT"
                });
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
                app.UseWebApi(config);
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test/action");
                var cookie = response.Headers.GetValues("Set-Cookie").First();
                Assert.That(cookie, Is.EqualTo("ACT=%7B%22action%22%3A%22FromAction%22%7D; domain=.localhost; path=/"));
            }
        }
    }

    [RoutePrefix("test")]
    public class ActionCookieTestController : ApiController
    {
        [HttpGet, Route("challenge")]
        public IHttpActionResult Challenge()
        {
            this.ChallengeActionMiddleware(new ActionData { Action = "FromChallenge" });
            return Ok();
        }

        [HttpGet, Route("action")]
        public IHttpActionResult Action()
        {
            return this.Action(new ActionData { Action = "FromAction" });
        }
    }
}
