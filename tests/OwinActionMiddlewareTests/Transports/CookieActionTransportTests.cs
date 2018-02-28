using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using OwinActionMiddleware;

namespace OwinActionMiddlewareTests.Transports
{
    [TestFixture]
    public class CookieActionTransportTests
    {
        [Test]
        public void CookieActionTransport_Constructor_PassNullOrEmptyCookieName_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CookieActionTransport(null));
            Assert.That(ex.ParamName, Is.EqualTo("cookieName"));

            ex = Assert.Throws<ArgumentNullException>(() => new CookieActionTransport(string.Empty));
            Assert.That(ex.ParamName, Is.EqualTo("cookieName"));
        }

        [Test]
        public async Task CookieActionTransport_Invoke_GivenCookieName_ShouldSetCookieWithoutRedirect()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseActionMiddleware(new ActionMiddlewareOptions
                {
                    Transport = new CookieActionTransport("ACT123")
                });

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path != new PathString("/test")) await next();
                    ctx.ChallengeActionMiddleware(new ActionData { Action = "JumpAround" });
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                });
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
                var cookie = cookies.SingleOrDefault();
                Assert.That(cookie, Is.EqualTo("ACT123=%7B%22action%22%3A%22JumpAround%22%7D; domain=.localhost; path=/"));
            }
        }

        [Test]
        public async Task CookieActionTransport_Invoke_GivenCustomCookieDomain_ShouldSetCookieWithCustomDomain()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseActionMiddleware(new ActionMiddlewareOptions
                {
                    Transport = new CookieActionTransport("ACT", customCookieDomain: "web.localhost")
                });

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path != new PathString("/test")) await next();
                    ctx.ChallengeActionMiddleware(new ActionData { Action = "JumpAround" });
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                });
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
                var cookie = cookies.SingleOrDefault();
                Assert.That(cookie, Is.EqualTo("ACT=%7B%22action%22%3A%22JumpAround%22%7D; domain=web.localhost; path=/"));
            }
        }

        [Test]
        public async Task CookieActionTransport_Invoke_GivenCustomRedirectUrl_ShouldSetCookieAndRedirect()
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseActionMiddleware(new ActionMiddlewareOptions
                {
                    Transport = new CookieActionTransport("ACT", optionalRedirectUrl: new Uri("https://some.domain.com"))
                });

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path != new PathString("/test")) await next();
                    ctx.ChallengeActionMiddleware(new CustomActionData { Action = "JumpAround", FirstName = "James", LastName = "Badass" });
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                });
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
                Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
                var cookie = cookies.SingleOrDefault();
                Assert.That(cookie, Is.EqualTo("ACT=%7B%22firstName%22%3A%22James%22%2C%22lastName%22%3A%22Badass%22%2C%22action%22%3A%22JumpAround%22%7D; domain=.localhost; path=/"));
                Assert.That(response.Headers.Location, Is.Not.Null);
                Assert.That(response.Headers.Location, Is.EqualTo(new Uri("https://some.domain.com")));
            }
        }

        public class CustomActionData : ActionData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
