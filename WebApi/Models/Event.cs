using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime? UpdatedOn { get; set; } = DateTime.Now;
        public int? Status { get; set; }

        public string UID { get; set; }

        public int? CalendarId { get; set; }
        public virtual Calendar Calendar{get; set;}

        public int? EventType { get; set; }
        public virtual IList<EventUser> Participants{get; set;}

        public int? Companyid { get; set; }
        public virtual Company Company { get; set; }

    }

}
