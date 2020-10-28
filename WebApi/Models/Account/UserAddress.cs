using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class UserAddress
    {
        [Key]
        public long UserId { get; set; }
        public User User { get; set; }
        public Address Address { get; set; }
        public int Addressid { get; set; }
        public DateTimeOffset? LastUsedOn { get; set; }
    }
}
