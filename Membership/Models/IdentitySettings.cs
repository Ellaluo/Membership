using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Membership.Models
{
    public class IdentitySettings
    {
        public string Secret { get; set; }
        public int TokenLifeTime { get; set; }
    }
}
