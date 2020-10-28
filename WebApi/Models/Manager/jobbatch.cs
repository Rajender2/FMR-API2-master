using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Batch
    {
        public int AssessmentId { get; set; }
        public int? OrderById { get; set; }
    }

    public class jobbatches
    {
        public List<Batch> Assessments { get; set; }
    }
}
