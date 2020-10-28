using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{

    public class Ques
    {
        public int QuestionId { get; set; }
        public int? OrderById { get; set; }
    }
    public class jobquestions
    {
        public List<Ques> Questions { get; set; }
    }
}
