using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Middleware.Service.Utilities;

namespace Middleware.Client.Filters
{
    public class LanguageFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string language;
       
            var ls = context.HttpContext.Request.Headers.TryGetValue("language", out var languages);
            if (ls)
            {
                language = languages[0].Trim().ToLower();
                var supported = language == "en" || language == "fr";
                if (!supported)
                {
                    language = "en";
                }
            }
            else
            {
                language = "en";
            }
            context.ActionArguments["language"] = language;
            await next();

        }
    }
}   