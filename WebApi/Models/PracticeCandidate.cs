using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class PracticeCandidate
    {
        public int? Id { get; set; }
        public int QuestionId { get; set; }
        public string VideoFile { get; set; }
        public DateTime? UploadedOn { get; set; } = DateTime.Now;
        public int CandidateId { get; set; }
        public int? Status { get; set; }
    }
}
