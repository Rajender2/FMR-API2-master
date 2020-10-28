using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class profile
    {

        public string fullname { get; set; }
        public string phone { get; set; }
        public string phone2 { get; set; }
        public string address { get; set; }
        public int? city { get; set; }
        public string zipcode { get; set; }
    }
}
