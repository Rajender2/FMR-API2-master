using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class InviteCandidate
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int CandidateId { get; set; }
        public int JoborderId { get; set; }
        public virtual JobOrder JobOrder {get; set;}
        public DateTime? SentOn { get; set; } = DateTime.Now;
    }
}
