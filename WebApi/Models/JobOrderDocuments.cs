using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class JobOrderDocuments
    {
        public int Id { get; set; }
        public int JobOrderId { get; set; }
        public virtual JobOrder JobOrder { get; set; }
        public int DocumentId { get; set; }
        public virtual DocumentTemplate Document { get; set; }
    }
}
