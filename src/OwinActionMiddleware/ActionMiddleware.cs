using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinActionMiddleware
{
    internal class ActionMiddleware : OwinMiddleware
    {
        private readonly IActionTransport _transport;

        public ActionMiddleware(OwinMiddleware next, ActionMiddlewareOptions options)
            : base(next)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _transport = options.Transport ?? throw new ArgumentNullException(nameof(options.Transport));
        }

        public override async Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context).ConfigureAwait(false);

            var actionData = context.GetActionData();
            if (actionData == null) return;

            await _transport.Invoke(context, actionData).ConfigureAwait(false);
        }
    }
}
