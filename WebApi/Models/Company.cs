using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Created {get; set;}
        public DateTime? Updated { get; set; }
        public Guid? UID { get; set; }
        public int? AddressId { get; set; }
        public virtual Address Address { get; set; }
        
    }
}
