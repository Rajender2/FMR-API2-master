using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class evnt
    {
        public int Id { get; set; } = 0;
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public long[] Participantids { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public int? CalenderId {get; set;}
    }
}
