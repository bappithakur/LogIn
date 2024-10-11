using AutoMapper;
using Database.Data;
using Login.Microservice.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace Login.Microservice.Services.TokenServices
{
    public class GenrateToken
    {
        private readonly IConfiguration _configuration;
        private RefreshToken _RefreshToken;
        public GenrateToken(IConfiguration configuration)
        {
            _configuration = configuration;
            _RefreshToken = new RefreshToken();
        }
        public UserProfileDTO GenratetokenOnUserName(UserDTO userDto)
        {
            IdentityOptions _options = new IdentityOptions();
            var claim = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, userDto.Profile??userDto.DdUserId.ToString()),
                    new Claim("RoleName", userDto.RoleId.ToString()),
                    new Claim("RoleId", userDto.RoleId.ToString()),
                    new Claim("UserId", userDto.DdUserId.ToString()),
                    new Claim("DdTenentEntityId", userDto.DdTenentEntityId.ToString()), 
                    new Claim("DdTenentId", userDto.DdTenentId.ToString()),  
                    new Claim("Name", userDto.Name.ToString()),
                    new Claim("Profile", userDto.Profile!=null?userDto.Profile.ToString():String.Empty),
                    new Claim("CountryName", userDto.CountryName.ToString()),
                    new Claim("DdCountryId", userDto.DdCountryId.ToString()),       
                    new Claim("DdStateId", userDto.DdStateId.ToString()),
                    new Claim("DdStateName", userDto.DdStateName.ToString()),
                    new Claim("CropUom", userDto.CropUom.ToString()),
                    new Claim("LandUom", userDto.LandUom.ToString()),
                    new Claim("BpBpartnerId",userDto.BpBpartnerId!=null? userDto.BpBpartnerId.ToString():"0"),
                    new Claim("HrMemberId", userDto.BpBpartnerId!=null?userDto.HrMemberId.ToString():"0"),
                };
            var signinKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));

            int expiryInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);
            //issuer: "http://abc.in",
            //audience: "http://abc.in",
            var token = new JwtSecurityToken(
              claims: claim,
              expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
              signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
            );
            var RefreshToken = _RefreshToken.GenrateRefreshToken(userDto.DdUserId.ToString());
            return
                  new UserProfileDTO
                  {
                      UserToken = new JwtSecurityTokenHandler().WriteToken(token),
                      Expiration = token.ValidTo.ToLocalTime(),
                      UserRefreshToken = RefreshToken,
                      UserProfileName = userDto.Name,
                     
                  };
        }
       
    }
}
