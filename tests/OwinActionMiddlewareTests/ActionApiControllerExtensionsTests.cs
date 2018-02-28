using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using OwinActionMiddleware;

namespace OwinActionMiddlewareTests
{
    [TestFixture]
    public class ActionApiControllerExtensionsTests
    {
        [Test]
        public async Task ChallengeActionMiddleware_ShouldAddDataToOwinEnvironment()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseActionMiddleware(new ActionMiddlewareOptions
                {
                    Transport = new CookieActionTransport("ACT", optionalRedirectUrl: new Uri("https://test.server.com"))
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
                app.UseActionMiddleware(new ActionMiddlewareOptions
                {
                    Transport = new CookieActionTransport("ACT", optionalRedirectUrl: new Uri("https://test.server.com"))
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
    public class ActionTestController : ApiController
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
