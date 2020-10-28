using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class Country
    {
        public int id { get; set; }
        [Required]
        [StringLength(450)]
        public string Name { get; set; }    
        public IList<State> StatesOrProvinces { get; set; } = new List<State>();
    }
}
