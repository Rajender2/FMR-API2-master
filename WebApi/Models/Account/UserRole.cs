using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    public class UserRole : IdentityUserRole<long>
    {

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
