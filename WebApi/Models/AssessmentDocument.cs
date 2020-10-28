using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class AssessmentDocument
    {
        public int id { get; set; }
        public int AssessmentId { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedOn { get; set; } = DateTime.Now;
        public int UploadedBy { get; set; }
        

    }
}
