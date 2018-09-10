using System;
using System.Threading.Tasks;
using Membership.Models;

namespace Membership.Repositories
{
    public interface IUserRepository
    {
        Task<UserInfoEntity> GetUserByUsernameAsync(string username);
        Task<UserInfoEntity> GetUserByIdAsync(Guid id);
        Task<int> CreateUserAsync(UserInfoEntity userInfo);
        Task<int> UpdatePasswordAsync(UserInfoEntity userInfoEntity);
        Task<int> UpdateAsync(UserInfoEntity userInfoEntity);
    }
}
