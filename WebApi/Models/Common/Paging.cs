using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Paging
    {
        public int page { get; set; } = 1;
        public int size { get; set; } = 25;
        public string flow { get; set; }
        public string sort { get; set; } = "dsc";
        public string q { get; set; } = "";
        public int totalrecs { get; set; } = 0;
        public int totalpages { get; set; } = 0;
    }
}
