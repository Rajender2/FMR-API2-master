using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class question
    {
        public int Id { get; set; } = 0;
        public string QuestionTitle { get; set; }
        public int Duration { get; set; }
        public int? BufferTime { get; set; } = 0;
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int QuestionTypeId { get; set; }
    }
}
