using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{

    public class resques
    {
        public int id { get; set; }
    }


    public class jobresponses
    {
        public List<resques> Responses { get; set; }

    }
}
