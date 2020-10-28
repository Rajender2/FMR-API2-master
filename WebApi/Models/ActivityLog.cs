using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class ActivityLog
    {
      public int Id {get; set;}
      public string Acitvity {get; set;}

      public DateTime AddedOn {get; set;}

      public long UserId { get; set; }
      public User User { get; set; }

    }
}
