using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{

    public class BatchList
    {
        public int assessmentId { get; set; }
        public int batch { get; set; }
        public int orderId { get; set; }

    }
    public class BatchAssessments
    {
        public List<BatchList> Batches { get; set; }
    }
}
