using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class onboarding
    {
        public int Id { get; set; } = 0;
        public int AssesmentId { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public DateTime? JoiningDate {get; set;}
        public int StatusId {get; set;}

    }
}
