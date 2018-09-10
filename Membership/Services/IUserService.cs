using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Membership.Models;

namespace Membership.Services
{
    public interface IUserService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<UserInfoDto> CreateUserAsync(UserInfoDto userInfoDto);
        string GenerateToken(string username, string client);
        Task<int> UpdateUserPasswordAsync(UserInfoDto userInfoDto);
        Task<int> ActivateAsync(UserInfoDto userInfoDto, string clientId);
        Task<int> DeActivateAsync(UserInfoDto userInfoDto, string clientId);
        Task<UserInfoDto> GetUserByUsernameAsync(string username);
    }
}
