using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class PracticeQuestions
    {
        public int Id { get; set; }
        public string QuestionTitle { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }

    }
}
