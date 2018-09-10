using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace MembershipUnitTest.Controllers
{
    public class ClaimsPrincipalFixture : IDisposable
    {
        private const string Name = "mluoau@gmail.com";
        private const string ClaimRole = "Administrator";
        private const string Client = "A";

        public ClaimsPrincipal User { get; private set; }

        public ClaimsPrincipalFixture()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Name),
                new Claim(ClaimTypes.Role, ClaimRole),
                new Claim("aud", Client)
            }));
        }
        public void Dispose()
        {
            // clean up test data
        }
    }
}
