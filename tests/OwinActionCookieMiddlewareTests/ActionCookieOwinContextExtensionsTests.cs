using Microsoft.Owin;
using Moq;
using NUnit.Framework;
using OwinActionCookieMiddleware;

namespace OwinActionCookieMiddlewareTests
{
    [TestFixture]
    public class ActionCookieOwinContextExtensionsTests
    {
        [Test]
        public void ChallengeActionMiddleware_ShouldAddDataToEnvironment()
        {
            var context = new Mock<IOwinContext>();
            var data = new ActionData { Action = "ClapYourHands" };
            var collectedKey = string.Empty;
            context.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((key, _) => { collectedKey = key; });

            context.Object.ChallengeActionMiddleware(data);

            context.Verify(x => x.Set(It.IsAny<string>(), data), Times.Once);
            Assert.That(collectedKey, Does.StartWith("ChallengeActionCookie."));
        }

        [Test]
        public void GetActionData_GivenChallengeActionMiddlewareWasCalled_ShouldGetDataFromEnvironment()
        {
            var context = new OwinContext();
            context.ChallengeActionMiddleware(new ActionData { Action = "ClapYourHands" });
            var data = context.GetActionData();
            Assert.That(data.Action, Is.EqualTo("ClapYourHands"));
        }

        [Test]
        public void GetActionData_GivenChallengeActionMiddlewareWasNotCalled_ShouldReturnNull()
        {
            var context = new OwinContext();
            Assert.That(context.GetActionData(), Is.Null);
        }
    }
}
