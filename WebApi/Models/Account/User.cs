using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class User : IdentityUser<long>, IEntityWithTypedId<long>
    {
        public User()
        {
            CreatedOn = DateTimeOffset.Now;
            UpdatedOn = DateTimeOffset.Now;
        }
        public Guid UserGuid { get; set; }

        [Required]
        [StringLength(450)]
        public string FullName { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
        public int? DefaultAddressId { get; set; }
        public virtual Address DefaultAddress { get; set; }
        public string RefreshTokenHash { get; set; }
        public string Culture { get; set; }
        public string Photo { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public IList<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}
