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
        private readonly string _baseUrl;
        private readonly string _fragmentParameterName;
        private readonly bool _useBase64Encoding;
        private readonly JsonSerializerSettings _serializerSettings;

        public FragmentActionTransport(string baseUrl, string fragmentParameterName, bool useBase64Encoding = false)
        {
            if (string.IsNullOrEmpty(fragmentParameterName)) throw new ArgumentNullException(nameof(fragmentParameterName));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _fragmentParameterName = fragmentParameterName;
            _useBase64Encoding = useBase64Encoding;
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public Task Invoke(IOwinContext context, ActionData actionData)
        {
            var location = new StringBuilder(_baseUrl);
            if (!_baseUrl.EndsWith("#"))
            {
                location.Append(_baseUrl.Contains("#") ? "&" : "#");
            }

            var fragmentData = JsonConvert.SerializeObject(actionData, _serializerSettings);
            if (_useBase64Encoding) fragmentData = Convert.ToBase64String(Encoding.UTF8.GetBytes(fragmentData));
            location.Append(_fragmentParameterName);
            location.Append("=");
            location.Append(Uri.EscapeDataString(fragmentData));

            context.Response.Redirect(location.ToString());
            return Task.CompletedTask;
        }
    }
}
