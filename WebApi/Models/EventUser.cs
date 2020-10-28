using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class EventUser
    {

        public int Id { get; set; }

        public int Status { get; set; }
        public bool IsOrganizer { get; set; }

        public long ParticipantId { get; set; }
        public virtual User Participant { get; set; }

        public int EventId { get; set; }
        public virtual Event Event { get; set; }

    }
}
