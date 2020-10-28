using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Data
{
    public class AnswerDTO
    {
        public int assessmentid { get; set; }
        public int questionid { get; set; }
        public string response { get; set; }
    }

    public class StatusDTO
    {
        public int assessmentId { get; set; }
        public int statusId { get; set; }
    }

    public class Evaluate
    {
       public int responseid { get; set; }
       public int rating { get; set; }
       public string notes { get; set; }
    }

    public class BatchDTO
    {
        public int assessmentId { get; set; }
        public int batchid { get; set; }
    }

}
