using System;

namespace OwinActionMiddleware
{
    public class ActionMiddlewareOptions
    {
        public Uri ApplicationUrl { get; set; }
        public string CookieName { get; set; }
        public string CustomCookieDomain { get; set; }
    }
}
