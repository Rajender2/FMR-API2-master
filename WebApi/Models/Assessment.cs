using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Assessment
    {
        public int Id { get; set; }

        public int? TotalRating { get; set; }
        
        public int? AssessmentStatusId { get; set; }

        public int CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; }

        public int? CalendarId { get; set; }
        public virtual Calendar Calendar {get; set;}

        public int? OnBoardingId { get; set; }
        public virtual AssessmentOnBoarding OnBoarding { get; set; }

        public int JobOrderId { get; set; }
        public virtual JobOrder JobOrder { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? Batch { get; set; } = 0;

        public int? OrderById { get; set; }

        public string ResumePath { get; set; }

        public virtual IList<AssesmentCandidate> Responses { get; set; }
        public virtual IList<AssessmentDocument> Documents { get; set; }
        public virtual IList<AssessmentForm> Forms { get; set; }

    }
}
