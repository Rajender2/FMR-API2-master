using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class vwPractice
    {
        public int QuestionId { get; set; }
        public string QuestionTitle { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }
        public string VideoFile { get; set; }
        public DateTime? UploadedOn { get; set; }
        public int? CandidateId { get; set; }
        public int? Status { get; set; }
        public int? Id { get; set; }

    }
}
