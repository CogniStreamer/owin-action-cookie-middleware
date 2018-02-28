using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace OwinActionMiddleware
{
    public class FragmentActionTransport : IActionTransport
    {
        private readonly Uri _baseUrl;
        private readonly string _fragmentParameterName;
        private readonly JsonSerializerSettings _serializerSettings;

        public FragmentActionTransport(Uri baseUrl, string fragmentParameterName)
        {
            if (string.IsNullOrEmpty(fragmentParameterName)) throw new ArgumentNullException(nameof(fragmentParameterName));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _fragmentParameterName = fragmentParameterName;
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public Task Invoke(IOwinContext context, ActionData actionData)
        {
            var baseUrl = _baseUrl.ToString();
            var location = new StringBuilder(baseUrl);
            if (!baseUrl.EndsWith("#"))
            {
                location.Append(baseUrl.Contains("#") ? "&" : "#");
            }

            var fragmentData = JsonConvert.SerializeObject(actionData, _serializerSettings);
            location.Append(_fragmentParameterName);
            location.Append("=");
            location.Append(Uri.EscapeDataString(fragmentData));

            context.Response.Redirect(location.ToString());
            return Task.CompletedTask;
        }
    }
}
