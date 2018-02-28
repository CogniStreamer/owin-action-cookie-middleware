using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Owin;
using OwinActionMiddleware;

namespace OwinActionMiddlewareTests
{
    [TestFixture]
    public class ActionMiddlewareTests
    {
        [Test]
        public void ActionMiddleware_Constructor_PassNullAsOptions_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ActionMiddleware(null, null));
            Assert.That(ex.ParamName, Is.EqualTo("options"));
        }

        [Test]
        public void ActionMiddleware_Constructor_PassInvalidOptionValues_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ActionMiddleware(null,
                new ActionMiddlewareOptions
                {
                    Transport = null
                }));
            Assert.That(ex.ParamName, Is.EqualTo("Transport"));
        }

        [Test]
        public async Task ActionMiddleware_DontChallengeMiddleware_ShouldNotInvokeActionTransport()
        {
            var transportMock = new Mock<IActionTransport>();
            var options = new ActionMiddlewareOptions { Transport = transportMock.Object };

            using (var server = TestServer.Create(app =>
            {
                app.Use<ActionMiddleware>(options);

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
                transportMock.Verify(x => x.Invoke(It.IsAny<IOwinContext>(), It.IsAny<ActionData>()), Times.Never);
            }
        }

        [Test]
        public async Task ActionMiddleware_ChallengeMiddleware_ShouldInvokeActionTransport()
        {
            var transportMock = new Mock<IActionTransport>();
            var options = new ActionMiddlewareOptions { Transport = transportMock.Object };
            var data = new ActionData { Action = "JumpAround" };

            using (var server = TestServer.Create(app =>
            {
                app.Use<ActionMiddleware>(options);

                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path != new PathString("/test")) await next();
                    ctx.ChallengeActionMiddleware(data);
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                });
            }))
            using (var client = server.HttpClient)
            {
                var response = await client.GetAsync("/test");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                transportMock.Verify(x => x.Invoke(It.IsAny<IOwinContext>(), data), Times.Once);
            }
        }
    }
}
