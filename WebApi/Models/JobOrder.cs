using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class JobOrder
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public int? Openings { get; set; }
        public string Summary { get; set; }
        public string Notes { get; set; }
        public int? Status { get; set; }
        public DateTime? Created { get; set; } = DateTime.Now;
        public DateTime? Updated { get; set; }
        public DateTime? Published { get; set; }
        public bool IsActive { get; set; }
        public string InviteId { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public int? JobTypeId { get; set; }
        public virtual JobType JobType {get; set;}

        public long? UserId { get; set; }
        public virtual User User { get; set; }

        public long? ManagerId { get; set; }
        public virtual User Manager { get; set; }

        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public DateTime? Batch1 { get; set; }
        public DateTime? Batch2 { get; set; }
        public DateTime? Batch3 { get; set; }

        public virtual IList<JobOrderDocuments> JobOrderDocuments { get; set; }
        public virtual IList<JobCandidate> Candidates { get; set; }
        public virtual IList<JobQuestion> Questions { get; set; }
        public virtual IList<Assessment> Assessments { get; set; }
        
    }
}
