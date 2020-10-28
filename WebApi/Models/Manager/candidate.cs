using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class candidate
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public int? Rating { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public DateTime? DOB { get; set; }
        public string Twitter { get; set; }
        public string LinkedIn { get; set; }
        public string Address { get; set; }
        public int CityId { get; set; }
        public string Phone { get; set; }
        public string ZipCode { get; set; }

    }

   

}
