using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Cand
    {
        public int CandidateId { get; set; }
    }

    public class jobcandidates
    {
         public List<Cand> Candidates { get; set; }
    }
   

}
