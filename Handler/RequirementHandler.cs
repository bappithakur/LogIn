using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Utilities.SharedModel;

namespace Login.Microservice.Handler
{
    public class RequirementHandler : AuthorizationHandler<UserRequirement>
    {
        readonly IHttpContextAccessor actionContext = null;
        public RequirementHandler(IHttpContextAccessor _httpContextAccessor)
        {
            actionContext = _httpContextAccessor;
        }
      
        public override Task HandleAsync(AuthorizationHandlerContext context)
        {
            UserRequirement requirement = new UserRequirement();
            var result = HandleRequirementAsync(context, requirement);
            return result;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRequirement requirement)
        {
           
            var authHeader = actionContext.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var claimlist = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(authHeader))
            {
                var token = new JwtSecurityToken(authHeader);
                claimlist = token.Claims.ToDictionary(pair => pair.Type, pair => pair.Value);
            }
            else
            {
                APIResponse response = new();

                response.success = false;
                response.data = JsonConvert.SerializeObject(Array.Empty<string>());
                response.message = "Session Is Expired!!";
                byte[] responseByte =  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));

                return Task.CompletedTask;
            }
            if (claimlist.Count > 0)
            {
                if (Convert.ToInt32(claimlist["RoleId"].ToString()) == requirement._RoleId)
                    context.Succeed(requirement);
                else
                    return Task.CompletedTask;
            }
            return Task.CompletedTask;

        }
    }
}