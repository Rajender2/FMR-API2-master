using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class AssessmentForm
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Answer { get; set; }
        public int AssessmentId { get; set; }
    }
}
