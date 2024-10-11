using AutoMapper;
using Database.Data;
using Login.Microservice.DTOs;
using Login.Microservice.Services.TokenServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text.Json;
using System.Xml.Linq;
using Utilities.SharedModel;
using Utilities.SharedModel.SharedEnums;

namespace Login.Microservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        private AgroTechContext  _agroTechContext;
        private GenrateToken _GenrateTokenClass;
        

        public AuthController(IConfiguration configuration, AgroTechContext agroTechContext)
        {
            _configuration = configuration;
            _agroTechContext = agroTechContext;
            _GenrateTokenClass = new GenrateToken(configuration);
        }
        #region  Login
        [HttpGet("LoginGetKey")]
        public IActionResult LoginGetKey()
        {
            var seed = SecurityBAL.GetUniqueKey(8);
            if (seed != null)
            {
                response.success = true;
                response.data = JsonConvert.SerializeObject(seed);
                response.message = null;
                response.error = null;
                return Ok(response);

            }
            else
            {
                response.success = false;
                response.data = null;
                response.message = "Unable To Provide Key Please Try After Some Time";
                response.error = null;
                return BadRequest(response);
            }
        }
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDto  loginDto)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var _rt = new UserProfileDTO();
                    var AthenticatedUser = (from u in _agroTechContext.DdUsers
                                            where u.IsActive == true && u.EMail.ToLower().Trim().Equals(loginDto.UserName.ToLower().Trim())
                                            select u).FirstOrDefault();
                    if (AthenticatedUser != null)
                    {
                        #region Match PassWord
                        // Comparing Password With Seed
                        var SaltedPassword = SecurityBAL.EncryptSHA512(AthenticatedUser.Password + loginDto.seedKey);
                        if (SaltedPassword == loginDto.PassWord)
                        {
                            //Get TatentEntities
                            var TatentEntities = (from u in _agroTechContext.DdTenentEntities
                                                  where u.IsActive == true && u.DdTenentEntityId == AthenticatedUser.DdTenentEntityId
                                                  select u).FirstOrDefault();
                            var CountryDetails = (from u in _agroTechContext.DdCountries
                                                  where u.IsActive == true && u.DdCountryId == TatentEntities.DdCountryId
                                                  select u).FirstOrDefault();
                            var StateDetails = (from u in _agroTechContext.DdStates
                                                where u.IsActive == true && u.DdStateId == TatentEntities.DdStateId
                                                select u).FirstOrDefault();
                            //Get CountryDetails
                            DTOs.Profile profile = new DTOs.Profile
                            {
                                Name = AthenticatedUser.Name,
                                Email = AthenticatedUser.EMail,
                                Countryid = TatentEntities.DdCountryId,
                                Country = CountryDetails.Name,
                                DdStateId = TatentEntities.DdStateId,
                                DdStateName = StateDetails != null ? StateDetails.Name : string.Empty,
                                CropUom = TatentEntities.CropUom,
                                LandUom = TatentEntities.LandUom,
                                BpBpartnerId= AthenticatedUser.BpBpartnerId,
                                HrMemberId= AthenticatedUser.HrMemberId,
                            };
                           

                            UserDTO userDTO = new UserDTO
                            {
                                DdTenentEntityId = AthenticatedUser.DdTenentEntityId,
                                DdTenentId = AthenticatedUser.DdTenentId,
                                DdUserId = AthenticatedUser.DdUserId,
                                Name = AthenticatedUser.Name,
                                Profile = AthenticatedUser.Profile,
                                RoleId = AthenticatedUser.RoleId,
                                DdCountryId= TatentEntities.DdCountryId,
                                CountryName= CountryDetails.Name,
                                DdStateId= TatentEntities.DdStateId,
                                DdStateName = StateDetails != null ? StateDetails.Name : string.Empty,
                                CropUom = TatentEntities.CropUom,
                                LandUom= TatentEntities.LandUom,
                                BpBpartnerId = AthenticatedUser.BpBpartnerId,
                                HrMemberId = AthenticatedUser.HrMemberId,
                            };

                            _rt = _GenrateTokenClass.GenratetokenOnUserName(userDTO);
                            _rt.profile = profile;
                            UserLog userLog = new UserLog
                            {

                                UserId = (int)userDTO.DdUserId,
                                RoleId = (int)userDTO.RoleId,
                                Jwttoken = _rt.UserToken,
                                JwtcreationDate = DateTime.Now,
                                JwtexpieryDate = _rt.Expiration,
                                RefreshToken = _rt.UserRefreshToken,
                                RtcreationDate = DateTime.Now,
                                RtexpieryDate = DateTime.Now.AddMinutes(300),
                                LoginStatus = (int)LoginStatus.ActiveSession,
                                LoginStatusUpdationDate = DateTime.Now,
                                Mac = loginDto.MAC,
                                Medium = loginDto.Medium,
                                BrowzerAndroidVersion = loginDto.Browser_android_version,
                                CreatedBy = (int)AthenticatedUser.DdUserId,
                                CreatedOn = DateTime.Now,
                                Ip = loginDto.IP,
                                IsActive = true,

                            };
                            _agroTechContext.UserLogs.Add(userLog);
                            var result = _agroTechContext.SaveChanges();
                            if (result >= 1)
                            {
                               
                                response.success = true;
                                response.data = JsonConvert.SerializeObject(_rt);
                                response.message = null;
                                response.error = null;
                                return Ok(response);
                            }
                            else
                            {
                                response.success = false;
                                response.data = null;
                                response.message = "Unable to Create Log!!! Retry";
                                response.error = null;
                                return BadRequest(response);

                            }
                        }
                        else
                        {
                            response.success = false;
                            response.data = null;
                            response.message = "The user name or password provided is incorrect.";
                            response.error = null;
                            return BadRequest(response);

                        }

                    }
                    else
                    {
                        response.success = false;
                        response.data = null;
                        response.message = "The user name or password provided is incorrect.";
                        response.error = null;
                        return BadRequest(response);


                    }
                }
                else
                {
                    response.success = false;
                    response.data = null;
                    response.message = "The user name or password provided is incorrect.";
                    response.error = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage));
                    return BadRequest(response);


                }
            }

            #endregion

            catch (Exception Ex)
            {
                response.success = false;
                response.data = null;
                response.message = "Unable to Process Request";
                response.error = Ex.Message;
                return BadRequest(response);
            }

        }
        [HttpPost("LoginDevTeamPostMan")]
        public IActionResult LoginDevTeamPostMan([FromBody] LoginDto loginDto)
        {
           
            try
            {
                if (ModelState.IsValid)
                {
                    var _rt = new UserProfileDTO();
                    var AthenticatedUser = (from u in _agroTechContext.DdUsers
                                            where u.IsActive == true && u.EMail.ToLower().Trim().Equals(loginDto.UserName.ToLower().Trim())
                                            select u).FirstOrDefault();
                    if (AthenticatedUser != null)
                    {
                        #region Match PassWord
                        // Comparing Password With Seed
                        loginDto.PassWord = SecurityBAL.EncryptSHA512(loginDto.PassWord);
                        loginDto.PassWord = SecurityBAL.EncryptSHA512(loginDto.PassWord + loginDto.seedKey);
                        var SaltedPassword = SecurityBAL.EncryptSHA512(AthenticatedUser.Password + loginDto.seedKey);
                        if (SaltedPassword == loginDto.PassWord)
                        {
                            //Get TatentEntities
                            var TatentEntities = (from u in _agroTechContext.DdTenentEntities
                                                  where u.IsActive == true && u.DdTenentEntityId == AthenticatedUser.DdTenentEntityId
                                                  select u).FirstOrDefault();
                            var CountryDetails = (from u in _agroTechContext.DdCountries
                                                  where u.IsActive == true && u.DdCountryId == TatentEntities.DdCountryId
                                                  select u).FirstOrDefault();
                            var StateDetails = (from u in _agroTechContext.DdStates
                                                where u.IsActive == true && u.DdStateId == TatentEntities.DdStateId
                                                select u).FirstOrDefault();

                            DTOs.Profile profile = new DTOs.Profile
                            {
                                Name = AthenticatedUser.Name,
                                Email = AthenticatedUser.EMail,
                                Countryid = TatentEntities.DdCountryId,
                                Country = CountryDetails.Name,
                                DdStateId = TatentEntities.DdStateId,
                                DdStateName = StateDetails != null ? StateDetails.Name : string.Empty,
                                CropUom = TatentEntities.CropUom,
                                LandUom = TatentEntities.LandUom,
                                BpBpartnerId = AthenticatedUser.BpBpartnerId,
                                HrMemberId = AthenticatedUser.HrMemberId,
                            };

                            UserDTO userDTO = new UserDTO
                            {
                                DdTenentEntityId = AthenticatedUser.DdTenentEntityId,
                                DdTenentId = AthenticatedUser.DdTenentId,
                                DdUserId = AthenticatedUser.DdUserId,
                                Name = AthenticatedUser.Name,
                                Profile = AthenticatedUser.Profile,
                                RoleId = AthenticatedUser.RoleId,
                                DdCountryId = TatentEntities.DdCountryId,
                                CountryName = CountryDetails.Name,
                                DdStateId = TatentEntities.DdStateId,
                                DdStateName = StateDetails != null ? StateDetails.Name : string.Empty,
                                CropUom = TatentEntities.CropUom,
                                LandUom = TatentEntities.LandUom,
                                BpBpartnerId = AthenticatedUser.BpBpartnerId,
                                HrMemberId = AthenticatedUser.HrMemberId,
                            };

                            _rt = _GenrateTokenClass.GenratetokenOnUserName(userDTO);
                            _rt.profile = profile;
                            UserLog userLog = new UserLog
                            {
         
                                UserId=(int) userDTO.DdUserId,
                                RoleId=(int) userDTO.RoleId,
                                Jwttoken= _rt.UserToken,
                                JwtcreationDate= DateTime.Now,
                                JwtexpieryDate= _rt.Expiration,
                                RefreshToken= _rt.UserRefreshToken,
                                RtcreationDate= DateTime.Now,
                                RtexpieryDate= DateTime.Now.AddMinutes(300),
                                LoginStatus= (int)LoginStatus.ActiveSession,
                                LoginStatusUpdationDate= DateTime.Now,
                                Mac = loginDto.MAC,
                                Medium = loginDto.Medium,
                                BrowzerAndroidVersion = loginDto.Browser_android_version,
                                CreatedBy = (int)AthenticatedUser.DdUserId,
                                CreatedOn = DateTime.Now,
                                Ip = loginDto.IP,
                                IsActive = true,

                            };
                            _agroTechContext.UserLogs.Add(userLog);
                            var result = _agroTechContext.SaveChanges();
                            if (result >= 1)
                            {
                                response.success = true;
                                response.data = JsonConvert.SerializeObject(_rt);
                                response.message = null;
                                response.error = null;
                                return Ok(response);
                            }
                            else
                            {
                                response.success = false;
                                response.data = null;
                                response.message = "Unable to Create Log!!! Retry";
                                response.error = null;
                                return BadRequest(response);

                            }
                        }
                        else
                        {
                            response.success = false;
                            response.data = null;
                            response.message = "The user name or password provided is incorrect.";
                            response.error = null;
                            return BadRequest(response);

                        }

                    }
                    else
                    {
                        response.success = false;
                        response.data = null;
                        response.message = "The user name or password provided is incorrect.";
                        response.error = null;
                        return BadRequest(response);
                        

                    }
                }
                else
                {
                    response.success = false;
                    response.data = null;
                    response.message = "The user name or password provided is incorrect.";
                    response.error = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage));
                    return BadRequest(response);
                    

                }
            }

            #endregion

            catch (Exception Ex)
            {
                response.success = false;
                response.data = null;
                response.message = "Unable to Process Request";
                response.error = Ex.Message;
                return BadRequest(response);
            }

        }
        #endregion

        #region Common For All Logins
        [NonAction]
        public static string generateID()
        {
            long i = 1;

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }

            string number = String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
            string s = number.Remove(6);

            return s;
        }

        [NonAction]
        public int Logout(LogOut _logOut, int status)
        {
            RefreshToken _RefreshToken = new RefreshToken();
            try
            {
                string[] data = _RefreshToken.Decrypt(_logOut.UserRefreshToken).Split("-");

                var UserId = Convert.ToInt32(data[1].ToString());
                var Status = status;
                var JWT =  _logOut.UserToken;
                var RefreshToken  = _logOut.UserRefreshToken;
                var logs = (from ul in _agroTechContext.UserLogs
                                 where ul.IsActive == true 
                                 && ul.UserId == UserId 
                                 select ul).ToList();
                if (logs.Count() == 1)
                {
                    var logDetails = (from ul in _agroTechContext.UserLogs
                                      where ul.IsActive == true && ul.LoginStatus == Status
                                      && ul.UserId == UserId && ul.Jwttoken == JWT && ul.RefreshToken == RefreshToken
                                      select ul).FirstOrDefault();
                    if (logDetails != null)
                    {
                        logDetails.IsActive = false;
                        logDetails.LoginStatus = status;
                        _agroTechContext.UserLogs.Update(logDetails);
                        var result = _agroTechContext.SaveChanges();
                        result = result >= 1 ?  1 :  0;
                        return result;
                    }
                    else
                        return 0;
                }
                else
                {

                    logs.ForEach(x => {
                        x.IsActive = false;
                        x.LoginStatus = status;
                    });
                    var result = _agroTechContext.SaveChanges();
                    result = result >= 1 ? 1 : 0;
                    return result;
                }
            }
            catch (Exception Ex)
            {
                return -10;
            }
        }

       
        [HttpPost("TokenWithRefreshToken")]
        public  IActionResult TokenWithRefreshToken([FromBody] UpdateUserProfile _UpdateUserProfile)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    int expiryInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);
                    int RefreshTokenExpiryInMinutes = Convert.ToInt32(_configuration["RefreshTokenExpiryInMinutes"]);
                    RefreshToken _RefreshToken = new RefreshToken();
                    var token = new JwtSecurityToken(_UpdateUserProfile.UserToken);
                    var tokenValid = token.ValidTo;
                    var claimlist = token.Claims.ToList();
                    var res = token.Claims.ToDictionary(pair => pair.Type, pair => pair.Value);
                    TimeSpan ts = (DateTime.UtcNow - Convert.ToDateTime(tokenValid));
                    string[] data = _RefreshToken.Decrypt(_UpdateUserProfile.UserRefreshToken).Split("-");
                    int UserID = Convert.ToInt32(data[1].ToString());
                    TimeSpan ts1 = DateTime.Now - Convert.ToDateTime(data[0].ToString());

                    if (ts.TotalMinutes > 0 && ts1.TotalMinutes < RefreshTokenExpiryInMinutes && ts1.TotalMinutes > 0)
                    {
                       
                        var userDto = new UserDTO
                        {
                            DdTenentEntityId = Convert.ToDecimal(res["DdTenentEntityId"].ToString()),
                            DdTenentId = Convert.ToDecimal(res["DdTenentId"].ToString()),
                            DdUserId = Convert.ToDecimal(res["DdUserId"].ToString()),
                            Name = res["Name"].ToString(),
                            Profile = res["Profile"].ToString(),
                            RoleId = Convert.ToDecimal(res["RoleId"].ToString()),
                            DdCountryId = Convert.ToDecimal(res["DdCountryId"].ToString()),
                            CountryName = res["CountryName"].ToString(),
                            DdStateId = Convert.ToDecimal(res["DdStateId"].ToString()),
                            DdStateName = res["DdStateName"].ToString(),
                            CropUom = Convert.ToDecimal(res["CropUom"].ToString()),
                            LandUom = Convert.ToDecimal(res["LandUom"].ToString()),
                            BpBpartnerId = Convert.ToDecimal(res["BpBpartnerId"].ToString()),
                            HrMemberId = Convert.ToDecimal(res["HrMemberId"].ToString()),
                        };
                        var AthenticatedUser = (from u in _agroTechContext.DdUsers
                                                where u.IsActive == true && u.DdUserId == userDto.DdUserId
                                                select u).FirstOrDefault();
                        var TatentEntities = (from u in _agroTechContext.DdTenentEntities
                                              where u.IsActive == true && u.DdTenentEntityId == AthenticatedUser.DdTenentEntityId
                                              select u).FirstOrDefault();
                        var CountryDetails = (from u in _agroTechContext.DdCountries
                                              where u.IsActive == true && u.DdCountryId == TatentEntities.DdCountryId
                                              select u).FirstOrDefault();
                        var StateDetails = (from u in _agroTechContext.DdStates
                                            where u.IsActive == true && u.DdStateId == TatentEntities.DdStateId
                                            select u).FirstOrDefault();
                        var _rt = _GenrateTokenClass.GenratetokenOnUserName(userDto);
                        LogOut _logOut = new LogOut();
                        _logOut.UserToken = _UpdateUserProfile.UserToken;
                        _logOut.UserRefreshToken = _UpdateUserProfile.UserRefreshToken;

                        int i = Convert.ToInt32(Logout(_logOut, (int)LoginStatus.InavtiveSession));
                        if (i > 0)
                        {

                            DTOs.Profile profile = new DTOs.Profile
                            {
                                Name = AthenticatedUser.Name,
                                Email = AthenticatedUser.EMail,
                                Countryid = TatentEntities.DdCountryId,
                                Country = CountryDetails.Name,
                            };
                            _rt.profile = profile;
                            response.success = true;
                            response.data = JsonConvert.SerializeObject(_rt);
                            response.message = null;
                            response.error = null;
                            return Ok(response);
                        }
                        else
                        {
                            response.success = false;
                            response.data = null;
                            response.message = "Token InValid!";
                            response.error = null;
                            return StatusCode(403, response);
                        }
                    }
                    else if (ts.TotalMinutes > expiryInMinutes && ts1.TotalMinutes > RefreshTokenExpiryInMinutes)
                    {
                        LogOut _logOut = new LogOut();
                        _logOut.UserToken = _UpdateUserProfile.UserToken;
                        _logOut.UserRefreshToken = _UpdateUserProfile.UserRefreshToken;

                        int i = Convert.ToInt32(Logout(_logOut,(int)LoginStatus.ForcedLogout));
                        if (i > 0)
                        {
                            response.success = false;
                            response.data = null;
                            response.message = " Logout of All Sessions .Session Expired Please Relogin!!";
                            response.error = null;
                            return StatusCode(403, response);
                        }
                        else
                        {
                            response.success = false;
                            response.data = null;
                            response.message = " Invalid Details!!";
                            response.error = null;
                            return StatusCode(403, response);
                        }

                    }
                    else
                    {
                        response.success = false;
                        response.data = null;
                        response.message = "Token Still Valid,Session is Active, Please wait for some time!";
                        response.error = null;
                        return StatusCode(403, response);
                    }
                }
                else
                {
                    response.success = false;
                    response.data = null;
                    response.message = "not a valid information.";
                    response.error = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage));
                    return BadRequest(response);
                }
            }
            catch (Exception Ex)
            {
                var Var = "Exception :" + Ex.ToString();
                return StatusCode(403, Var);
            }
        }


        [Authorize(Policy = "ContentsEditor")]
        [HttpPost("LogOut")]
        public async Task<IActionResult> LogOut([FromBody] LogOut _logOut)
        {
            int i = Logout(_logOut, 2);
            if (Convert.ToInt32(i) > 0)
            {
                return Ok("Logout Successfully");
            }
            else
            {
                return StatusCode(403, "Token InValid!");
            }
        }

        [Authorize(Policy = "ContentsEditor")]
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword  changePassword)
        {
            if (changePassword == null) return BadRequest("Invalid Data!!!");
            else
            {
                int result = 0;
                var UserId = GetUserId();
                var AthenticatedUser = (from u in _agroTechContext.DdUsers
                                        where u.IsActive == true && u.DdUserId== UserId
                                        select u).FirstOrDefault();
                var SaltedPasswordUSer = SecurityBAL.EncryptSHA512(AthenticatedUser.Password + changePassword.seedKey);
                var SaltedPassword = SecurityBAL.EncryptSHA512(AthenticatedUser.Password + changePassword.seedKey);
                if (AthenticatedUser != null && SaltedPasswordUSer== SaltedPassword)
                {
                    AthenticatedUser.Password = changePassword.NewPassword;
                    AthenticatedUser.UpdatedBy = UserId;
                    AthenticatedUser.UpdatedOn = DateTime.Now;
                    _agroTechContext.DdUsers.Update(AthenticatedUser);
                    result = _agroTechContext.SaveChanges();
                }
                //if (AthenticatedUser!=null && changePassword.OldPassword.Trim().Equals(SaltedPassword.Trim()))
                //{
                //    AthenticatedUser.Password = changePassword.NewPassword;
                //    AthenticatedUser.UpdatedBy = UserId;
                //    AthenticatedUser.UpdatedOn=DateTime.Now;
                //    _agroTechContext.DdUsers.Update(AthenticatedUser);
                //    result =_agroTechContext.SaveChanges();
                //}
               
                if (Convert.ToInt32(result) > 0)
                {
                    response.success = true;
                    response.data = JsonConvert.SerializeObject(result);
                    response.message = "Password changed Successfully";
                    response.error = null;
                    return Ok(response);
                }
                else
                {
                    response.success = false;
                    response.data = null;
                    response.message = "Try again";
                    response.error = null;
                    return BadRequest(response);
                    
                }
            }
        }
        #endregion
       

    }
}
