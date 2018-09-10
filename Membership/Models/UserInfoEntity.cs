using System;

namespace Membership.Models
{
    public class UserInfoEntity
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool ActivateStatusA { get; set; }
        public bool ActivateStatusB { get; set; }
        public bool ActivateStatusC { get; set; }
    }
}
