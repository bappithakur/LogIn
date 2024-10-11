using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Login.Microservice.DTOs
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class LoginDto  
    {
        
        [Required(ErrorMessage = "UserName is required.")]
        public string UserName  { get; set; }
        [Required(ErrorMessage = "PassWord is required.")]
        public string PassWord { get; set; }
        [Required(ErrorMessage = "seed Key is required.")]
        public string? seedKey { get; set; }
        public string? IP { get; set; }
        public string? MAC { get; set; }
        public string? Medium { get; set; }
        public string? Browser_android_version { get; set; }
    }
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserProfileDTO  
    {
        // All properties Rename With Camel Rule and Profile Name AddOn
        public string? UserToken { get; set; }
        public DateTime Expiration { get; set; }
        public string? UserRefreshToken { get; set; }
        public string? UserProfileName { get; set; }
        public  Profile? profile { get; set; }
    }
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Profile
    {
        public string?  Name { get; set; }
        public string? Email { get; set; }
        public string? Country { get; set; }
        public decimal? Countryid { get; set; }
        public decimal? DdStateId { get; set; }
        public string? DdStateName { get; set; }
        public decimal? LandUom { get; set; }
        public decimal? CropUom { get; set; }
        public decimal? HrMemberId { get; set; }
        public decimal? BpBpartnerId { get; set; }
    }
       
public class UpdateUserProfile //old Name GenrateTokenByRefreshToken
    {
        // All properties Rename With Camel Rule and Profile Name AddOn
        public string? UserToken { get; set; }
        public string? UserRefreshToken { get; set; }
        public string? IP { get; set; }
        public string? MAC { get; set; }
        public string? Medium { get; set; }
        public string? Browser_android_version { get; set; }
    }
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class LogOut //old Name LogOutM
    {
        // All properties Rename With Camel Rule and Profile Name AddOn
        public string? UserToken { get; set; }
        public string? UserRefreshToken { get; set; }
    }
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ChangePassword  //old Name LogOutM
    {
        // All properties Rename With Camel Rule and Profile Name AddOn
        public string? OldPassword  { get; set; }
        public string? NewPassword  { get; set; }
        public string? seedKey { get; set; }
    }
    public class UserDTO
    {
        public decimal DdUserId { get; set; }
        public decimal DdTenentId { get; set; }
        public decimal DdTenentEntityId { get; set; }
        public string Name { get; set; } = null!;       
        public string? Profile { get; set; }
        public decimal RoleId { get; set; }
        public string? CountryName { get; set; } = null!;
        public decimal? DdCountryId { get; set; }
        public decimal? DdStateId { get; set; }
        public string? DdStateName { get; set; }
        public decimal? LandUom { get; set; }
        public decimal? CropUom { get; set; }
        public decimal? HrMemberId { get; set; }
        public decimal? BpBpartnerId { get; set; }
    }

   
}
