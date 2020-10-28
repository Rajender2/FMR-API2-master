using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Job
    {
        public int Id { get; set; } = 0;
        public string Title { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public int? Openings { get; set; }
        public string Summary { get; set; }
        public string Notes { get; set; }
        public int? JobTypeId { get; set; }
        public int? Status { get; set; }
        public long? ManagerId { get; set; }
        public bool IsActive { get; set; }
        public List<int> RequiredDocs { get; set; }
        public string Location { get; set; }
        public string CompanyName { get; set; }

    }
}
