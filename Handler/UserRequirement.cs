using Microsoft.AspNetCore.Authorization;

namespace Login.Microservice.Handler
{
    public class UserRequirement : IAuthorizationRequirement
    {
        public int _RoleId { get; }
        public int _UserId { get; }
        public UserRequirement(int RoleId = 0, int UserId = 0)
        {
            _RoleId = RoleId;
            _UserId = UserId;

        }
    }
}
