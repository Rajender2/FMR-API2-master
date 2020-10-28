using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class AssesmentCandidate
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Notes { get; set; }
        public string Videofile { get; set; }
        public DateTime? UploadedOn { get; set; } = DateTime.Now;
        public long? AddedBy { get; set; }
        public int AssessmentId { get; set; }
        public int QuestionId { get; set; }
        public virtual Question Question{get; set;}
        public long? EvaluatedBy { get; set; }
        public int Status { get; set; }
    }
}
