using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class QuestionType
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }

        public long UserId { get; set; }
        public int? CompanyId { get; set; }
    }
}
