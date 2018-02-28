using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace OwinActionMiddleware
{
    public class CookieActionTransport : IActionTransport
    {
        private readonly string _cookieName;
        private readonly string _customCookieDomain;
        private readonly Uri _optionalRedirectUrl;
        private readonly JsonSerializerSettings _serializerSettings;

        public CookieActionTransport(string cookieName, string customCookieDomain = null, Uri optionalRedirectUrl = null)
        {
            if (string.IsNullOrEmpty(cookieName)) throw new ArgumentNullException(nameof(cookieName));
            _cookieName = cookieName;
            _customCookieDomain = customCookieDomain;
            _optionalRedirectUrl = optionalRedirectUrl;
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public Task Invoke(IOwinContext context, ActionData actionData)
        {
            var cookieData = JsonConvert.SerializeObject(actionData, _serializerSettings);

            context.Response.Cookies.Append(_cookieName,
                cookieData,
                new CookieOptions
                {
                    Domain = _customCookieDomain ?? GetDefaultCookieDomainForContext(context),
                    HttpOnly = false,
                    Secure = false
                });

            if (_optionalRedirectUrl != null) context.Response.Redirect(_optionalRedirectUrl.ToString());

            return Task.CompletedTask;
        }

        private string GetDefaultCookieDomainForContext(IOwinContext context) => $".{context.Request.Uri.Host}";
    }
}
