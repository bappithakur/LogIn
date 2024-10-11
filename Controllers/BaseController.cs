using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Utilities.SharedModel;

namespace Login.Microservice.Controllers
{
   
    public abstract class BaseController : ControllerBase
    {
        public APIResponse response = new();
        JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        [NonAction]
        public decimal GetUserId()
        {

            return decimal.Parse(this.User.Claims.First(i => i.Type == "UserId").Value);

        }

        [NonAction]
        public decimal GetDDCountryID()
        {

            return decimal.Parse(this.User.Claims.First(i => i.Type == "DdCountryId").Value);

        }
    }
}
