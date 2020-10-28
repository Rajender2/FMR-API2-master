using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Calendar
    {
        public int Id { get; set; }
        public DateTime? AddedOn { get; set; }
        public bool? InviteSent { get; set; } = false;
        public DateTime? SentOn { get; set; }
        public long? SentBy { get; set; }
        public virtual IList<Event> Events {get; set;}
    }
}
