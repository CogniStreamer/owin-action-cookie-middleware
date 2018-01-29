using System;

namespace OwinActionCookieMiddleware
{
    public class ActionCookieMiddlewareOptions
    {
        public Uri ApplicationUrl { get; set; }
        public string CookieName { get; set; }
        public string CustomCookieDomain { get; set; }
    }
}