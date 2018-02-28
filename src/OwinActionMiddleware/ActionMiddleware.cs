using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OwinActionMiddleware
{
    internal class ActionMiddleware : OwinMiddleware
    {
        private readonly Uri _applicationUrl;
        private readonly string _cookieName;
        private readonly string _customCookieDomain;
        private readonly JsonSerializerSettings _serializerSettings;

        public ActionMiddleware(OwinMiddleware next, ActionMiddlewareOptions options)
            : base(next)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.ApplicationUrl == null) throw new ArgumentNullException(nameof(options.ApplicationUrl));
            if (!options.ApplicationUrl.IsAbsoluteUri) throw new ArgumentException("Must be an absolute URL", nameof(options.ApplicationUrl));
            if (string.IsNullOrEmpty(options.CookieName)) throw new ArgumentNullException(nameof(options.CookieName));
            _applicationUrl = options.ApplicationUrl;
            _cookieName = options.CookieName;
            _customCookieDomain = options.CustomCookieDomain;
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public override async Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);

            var actionData = context.GetActionData();
            if (actionData == null) return;

            var cookieData = JsonConvert.SerializeObject(actionData, _serializerSettings);

            context.Response.Cookies.Append(_cookieName,
                cookieData,
                new CookieOptions
                {
                    Domain = _customCookieDomain ?? GetDefaultCookieDomainForContext(context),
                    HttpOnly = false,
                    Secure = false
                });

            context.Response.Redirect(_applicationUrl.ToString());
        }

        private string GetDefaultCookieDomainForContext(IOwinContext context)
            => _applicationUrl.Host != context.Request.Uri.Host ? $".{context.Request.Uri.Host}" : null;
    }
}
