using System.Threading.Tasks;
using Microsoft.Owin;

// ReSharper disable once CheckNamespace
namespace OwinActionMiddleware
{
    public interface IActionTransport
    {
        Task Invoke(IOwinContext context, ActionData actionData);
    }
}
