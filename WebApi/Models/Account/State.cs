using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class State 
    {
        public int id { get; set; }
        public string Code { get; set; }
        [Required]
        [StringLength(450)]
        public string Name { get; set; }
        public int Countryid { get; set; }
        public virtual Country Country { get; set; }
    }
}
