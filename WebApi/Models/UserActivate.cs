using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class UserActivate
    {
        public int Id {get; set; }
     
        public string GuiId { get; set; }
        public string Token {get; set;}
        public long UserId { get; set; }
        public virtual User User { get; set; }
    }
}
