using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class Address
    {
        public Address() { }
        public int id { get; set; }
        public string ContactName { get; set; }

        public string Phone { get; set; }

        public string AddressLine { get; set; }

        public string ZipCode { get; set; }
        
        public int? CityId { get; set; }
        public int CountryId { get; set; }
        public virtual City City { get; set; }


     
    }
}
