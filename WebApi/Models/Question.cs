using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Question
    {
        public int  Id {get; set;}
        public string QuestionTitle {get; set;}
        public int Duration {get; set;}
        public int? BufferTime { get; set; } = 0;
        public string Description {get; set;}
        public DateTime? Updated {get; set;}
        public bool IsActive {get; set;}
        public long UserId { get; set; }

        public int QuestionTypeId {get; set;}
        public virtual QuestionType QuestionType { get; set; }

    }
}
