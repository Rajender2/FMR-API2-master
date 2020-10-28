using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class vwCandidateJob
    {


        [Key]
        public int JobOrderId { get; set; }
        [Key]
        public int CandidateId { get; set; }
        [Key]
        public int QuestionId { get; set; }
        public int Duration { get; set; }
        public int? BufferTime { get; set; } = 0;
        public string Description { get; set; }
        public string Videofile { get; set; }
        public DateTime? UploadedOn { get; set; } = DateTime.Now;
        [Key]
        public int AssessmentId { get; set; }
        public string QuestionTitle { get; set; }
        public int? Rating { get; set; }
        public string Notes { get; set; }
        

        public int? OrderById { get; set; }
        public int? Status { get; set; }

        public int? Id { get; set; }

    }
}
