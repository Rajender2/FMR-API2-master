using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class JobCandidate
    {
        [Key]
        public int Id { get; set; }
        public int jobOrderId { get; set; }
        public virtual JobOrder JobOrder { get; set; }

        public int CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; }
        public DateTime? AddedOn { get; set; } = DateTime.Now;
        public long? AddedById { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
