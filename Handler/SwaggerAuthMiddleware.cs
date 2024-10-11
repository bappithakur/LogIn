using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Login.Microservice.Handler
{
    public class SwaggerAuthMiddleware 
    {
        private readonly RequestDelegate next;

 
        public SwaggerAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public  async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                var authHandler  = context.RequestServices.GetRequiredService<RequirementHandler>();
                if (authHandler != null)
                {
                    await next.Invoke(context).ConfigureAwait(false);
                    return;
                }
                
            }
            else
            {
                await next.Invoke(context).ConfigureAwait(false);
            }
        }

    }

    public static class RequestSwaggerAuthMiddlewareExtensions 
    {
        public static IApplicationBuilder UseRequestSwaggerAuth(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerAuthMiddleware>();
        }
    }
}
