using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string County { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int StateId { get; set; }
        public virtual State State { get; set; }
    }
}
