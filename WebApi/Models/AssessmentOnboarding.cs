using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class AssessmentOnBoarding
    {
        public int Id { get; set; }

        public DateTime? JoiningDate { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }

        public int StatusId { get; set; }
        public DateTime? AddedOn { get; set; }

        public long AddedById { get; set; }
        public virtual User AddedBy { get; set; }

    }
}
