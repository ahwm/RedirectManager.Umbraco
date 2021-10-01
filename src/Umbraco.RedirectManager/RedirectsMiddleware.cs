using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Web;

namespace RedirectManager
{
    public class RedirectsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly RedirectService _redirectService;

        public RedirectsMiddleware(RequestDelegate next, IUmbracoContextAccessor umbracoContextAccessor, RedirectService redirectService)
        {
            _next = next;
            _umbracoContextAccessor = umbracoContextAccessor;
            _redirectService = redirectService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string pathAndQuery = context.Request.GetEncodedPathAndQuery();
            Uri url = new Uri(context.Request.GetEncodedUrl());

            string primaryDomain = _redirectService.GetPrimaryDomain();
            if (!String.IsNullOrEmpty(primaryDomain) && url.Host != "localhost")
            {
                if (primaryDomain != url.Host)
                {
                    var uri = new UriBuilder(url);
                    uri.Host = primaryDomain;
                    context.Response.Redirect(uri.ToString(), true);
                    return;
                }
            }

            var redirect = _redirectService.GetRedirectByUrl(pathAndQuery);
            if (redirect == null)
            {
                await _next(context);
                return;
            }

            context.Response.Redirect(redirect, true);
        }
    }
}
