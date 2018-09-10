using Membership.Models;
using Microsoft.EntityFrameworkCore;

namespace Membership.Helper
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        }

        public DbSet<UserInfoEntity> UserInfoEntity { get; set; }
    }
}
