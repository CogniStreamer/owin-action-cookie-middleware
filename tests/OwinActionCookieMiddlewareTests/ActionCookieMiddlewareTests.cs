using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using OwinActionCookieMiddleware;

namespace OwinActionCookieMiddlewareTests
{
    [TestFixture]
    public class ActionCookieMiddlewareTests
    {
        [Test]
        public void ActionCookieMiddleware_Constructor_PassNullAsOptions_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ActionCookieMiddleware(null, null));
            Assert.That(ex.ParamName, Is.EqualTo("options"));
        }

        [Test]
        public void ActionCookieMiddleware_Constructor_PassInvalidOptionValues_ShouldThrowException()
        {
            ArgumentException ex;

            ex = Assert.Throws<ArgumentNullException>(() => new ActionCookieMiddleware(null,
                new ActionCookieMiddlewareOptions
                {
                    ApplicationUrl = null,
                    CookieName = "Name",
                    CustomCookieDomain = "Domain"
                }));
            Assert.That(ex.ParamName, Is.EqualTo("ApplicationUrl"));

            ex = Assert.Throws<ArgumentException>(() => new ActionCookieMiddleware(null,
                new ActionCookieMiddlewareOptions
                {
                    ApplicationUrl = new Uri("/test", UriKind.Relative),
                    CookieName = "Name",
                    CustomCookieDomain = "Domain"
                }));
            Assert.That(ex.Message, Does.StartWith("Must be an absolute URL"));
            Assert.That(ex.ParamName, Is.EqualTo("ApplicationUrl"));

            ex = Assert.Throws<ArgumentNullException>(() => new ActionCookieMiddleware(null,
                new ActionCookieMiddlewareOptions
                {
                    ApplicationUrl = new Uri("http://localhost"),
                    CookieName = null,
                    CustomCookieDomain = "Domain"
                }));
            Assert.That(ex.ParamName, Is.EqualTo("CookieName"));

            ex = Assert.Throws<ArgumentNullException>(() => new ActionCookieMiddleware(null,
                new ActionCookieMiddlewareOptions
                {
                    ApplicationUrl = new Uri("http://localhost"),
                    CookieName = string.Empty,
                    CustomCookieDomain = "Domain"
                }));
            Assert.That(ex.ParamName, Is.EqualTo("CookieName"));
        }

        [Test]
        public void ActionCookieMiddleware_Constructor_PassingNullAsCookieDomain_ShouldNotThrowException()
        {
            Assert.DoesNotThrow(() => new ActionCookieMiddleware(null,
                new ActionCookieMiddlewareOptions
                {
                    ApplicationUrl = new Uri("http://localhost"),
                    CookieName = "Name",
                    CustomCookieDomain = null
                }));
        }

        [Test]
        public async Task ActionCookieMiddleware_DontChallengeMiddleware_ShouldNotAddCookie()
        {
            var options = new ActionCookieMiddlewareOptions
            {
                ApplicationUrl = new Uri("https://test.server.com"),
                CookieName = "ACT"
            };

            using (var server = TestServer.Create(app =>
            {
                app.Use<ActionCookieMiddleware>(options);

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path != new PathString("/test")) await next();
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                });
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Headers.TryGetValues("Set-Cookie", out var _), Is.False);
            }
        }

        [Test]
        public async Task ActionCookieMiddleware_ChallengeMiddleware_ShouldAddCookieAndRedirect()
        {
            var options = new ActionCookieMiddlewareOptions
            {
                ApplicationUrl = new Uri("https://test.server.com"),
                CookieName = "ACT"
            };

            using (var server = TestServer.Create(app =>
            {
                app.Use<ActionCookieMiddleware>(options);

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
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

                Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
                var cookie = cookies.SingleOrDefault();
                Assert.That(cookie, Is.Not.Null);
                Assert.That(cookie, Is.EqualTo("ACT=%7B%22action%22%3A%22JumpAround%22%7D; domain=.localhost; path=/"));

                Assert.That(response.Headers.Location, Is.Not.Null);
                Assert.That(response.Headers.Location, Is.EqualTo(options.ApplicationUrl));
            }
        }

        [Test]
        public async Task ActionCookieMiddleware_PassCustomCookieDomain_ChallengeMiddleware_ShouldAddCookieWithCustomDomain()
        {
            var options = new ActionCookieMiddlewareOptions
            {
                ApplicationUrl = new Uri("https://test.server.com"),
                CookieName = "ACT",
                CustomCookieDomain = ".server.com"
            };

            using (var server = TestServer.Create(app =>
            {
                app.Use<ActionCookieMiddleware>(options);

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
                Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
                var cookie = cookies.SingleOrDefault();
                Assert.That(cookie, Is.Not.Null);
                Assert.That(cookie, Is.EqualTo("ACT=%7B%22action%22%3A%22JumpAround%22%7D; domain=.server.com; path=/"));
            }
        }

        [Test]
        public async Task ActionCookieMiddleware_PassCustomData_ChallengeMiddleware_ShouldAddCookieWithCustomData()
        {
            var options = new ActionCookieMiddlewareOptions
            {
                ApplicationUrl = new Uri("https://test.server.com"),
                CookieName = "ACT"
            };

            using (var server = TestServer.Create(app =>
            {
                app.Use<ActionCookieMiddleware>(options);

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path != new PathString("/test")) await next();
                    ctx.ChallengeActionMiddleware(new CustomActionData { Action = "JumpAround", FirstName = "Canon", LastName = "Ball" });
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                });
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test");
                Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
                var cookie = cookies.SingleOrDefault();
                Assert.That(cookie, Is.Not.Null);
                Assert.That(cookie, Is.EqualTo("ACT=%7B%22firstName%22%3A%22Canon%22%2C%22lastName%22%3A%22Ball%22%2C%22action%22%3A%22JumpAround%22%7D; domain=.localhost; path=/"));
            }
        }

        public class CustomActionData : ActionData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
