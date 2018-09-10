using System;
using System.Threading.Tasks;
using Membership.Helper;
using Membership.Models;
using Microsoft.EntityFrameworkCore;

namespace Membership.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApiContext _context;

        public UserRepository(ApiContext context)
        {
            _context = context;
        }

        public async Task<UserInfoEntity> GetUserByUsernameAsync(string username)
        {
            var user = await _context.UserInfoEntity.SingleOrDefaultAsync(u => u.Username == username);
            return user;
        }

        public async Task<UserInfoEntity> GetUserByIdAsync(Guid id)
        {
            var user = await _context.UserInfoEntity.SingleOrDefaultAsync(u => u.Id == id);
            return  user;
        }

        public async Task<int> CreateUserAsync(UserInfoEntity userInfoEntity)
        {
            _context.UserInfoEntity.Add(userInfoEntity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdatePasswordAsync(UserInfoEntity userInfoEntity)
        {
            var user = await _context.UserInfoEntity.SingleOrDefaultAsync(u => u.Id == userInfoEntity.Id);

            user.PasswordHash = userInfoEntity.PasswordHash;
            user.PasswordSalt = userInfoEntity.PasswordSalt;

            _context.UserInfoEntity.Update(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(UserInfoEntity userInfoEntity)
        {
            var user = await _context.UserInfoEntity.SingleOrDefaultAsync(u => u.Id == userInfoEntity.Id);

            user.ActivateStatusA = userInfoEntity.ActivateStatusA;
            user.ActivateStatusB = userInfoEntity.ActivateStatusB;
            user.ActivateStatusC = userInfoEntity.ActivateStatusC;

            _context.UserInfoEntity.Update(user);
            return await _context.SaveChangesAsync();
        }
    }
}
