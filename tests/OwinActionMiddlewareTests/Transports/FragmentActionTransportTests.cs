using System;
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
    public class FragmentActionTransportTests
    {
        [Test]
        public void FragmentActionTransport_Constructor_PassNullAsBaseUrl_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new FragmentActionTransport(null, "act"));
            Assert.That(ex.ParamName, Is.EqualTo("baseUrl"));
        }

        [Test]
        public void FragmentActionTransport_Constructor_PassNullOrEmptyFragmentParameterName_ShouldThrowException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new FragmentActionTransport("https://some.domain.com", null));
            Assert.That(ex.ParamName, Is.EqualTo("fragmentParameterName"));

            ex = Assert.Throws<ArgumentNullException>(() => new FragmentActionTransport("https://some.domain.com", string.Empty));
            Assert.That(ex.ParamName, Is.EqualTo("fragmentParameterName"));
        }

        [Test]
        public Task FragmentActionTransport_InvokeWithAbsoluteUriWithoutFragment_ShouldRedirectToBaseUrlWithActionInFragment()
            => BaseUrlTest("https://some.domain.com/", "https://some.domain.com/#act=%7B%22action%22%3A%22JumpAround%22%7D");

        [Test]
        public Task FragmentActionTransport_InvokeWithAbsoluteUriWithEmptyFragment_ShouldRedirectToBaseUrlWithActionInFragment()
            => BaseUrlTest("https://some.domain.com/#", "https://some.domain.com/#act=%7B%22action%22%3A%22JumpAround%22%7D");

        [Test]
        public Task FragmentActionTransport_InvokeWithAbsoluteUriWithExistingFragment_ShouldRedirectToBaseUrlWithActionAddedToExistingFragment()
            => BaseUrlTest("https://some.domain.com/#test=123", "https://some.domain.com/#test=123&act=%7B%22action%22%3A%22JumpAround%22%7D");

        [Test]
        public Task FragmentActionTransport_InvokeWithRelativeUriWithoutFragment_ShouldRedirectToBaseUrlWithActionInFragment()
            => BaseUrlTest("/test", "/test#act=%7B%22action%22%3A%22JumpAround%22%7D");

        [Test]
        public Task FragmentActionTransport_InvokeWithRelativeUriWithEmptyFragment_ShouldRedirectToBaseUrlWithActionInFragment()
            => BaseUrlTest("/test#", "/test#act=%7B%22action%22%3A%22JumpAround%22%7D");

        [Test]
        public Task FragmentActionTransport_InvokeWithRelativeUriWithExistingFragment_ShouldRedirectToBaseUrlWithActionAddedToExistingFragment()
            => BaseUrlTest("/test#one=two", "/test#one=two&act=%7B%22action%22%3A%22JumpAround%22%7D");

        private static async Task BaseUrlTest(string baseUrl, string expectedRedirectUrl)
        {
            using (var server = TestServer.Create(app =>
            {
                app.UseActionMiddleware(new ActionMiddlewareOptions
                {
                    Transport = new FragmentActionTransport(baseUrl, "act")
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
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
                Assert.That(response.Headers.Location, Is.Not.Null);
                Assert.That(response.Headers.Location.OriginalString, Is.EqualTo(expectedRedirectUrl));
            }
        }
    }
}
