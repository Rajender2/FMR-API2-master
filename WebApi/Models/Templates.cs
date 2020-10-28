using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class DocumentTemplate
    {
      public int Id { get; set; }
      public string DocumentName { get; set; }
    }

    public class FormTemplate
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public int? CreatedBy { get; set; }
        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? IsActive { get; set; }
    }
}
