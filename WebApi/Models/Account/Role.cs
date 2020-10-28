using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebApi.Models
{
    public class Role : IdentityRole<long>, IEntityWithTypedId<long>
    {

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
