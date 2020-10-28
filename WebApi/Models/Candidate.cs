using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public int? Rating { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public DateTime? DOB { get; set; }
        public string Twitter { get; set; }
        public string LinkedIn { get; set; }
        public DateTime? Created { get; set; } = DateTime.Now;
        public DateTime? Updated { get; set; } = DateTime.Now;
        public long? AddedBy { get; set; }

        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public long? UserId { get; set; }
        public virtual User User{get; set;}
        public int? Addressid { get; set; }
        public virtual Address Address { get; set; }

    }
}
