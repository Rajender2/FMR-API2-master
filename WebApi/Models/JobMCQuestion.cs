using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class JobMCQuestion
    {
        [Key]
        public int Id { get; set; }
        public int JobOrderId { get; set; }
       
        public DateTime AddedOn { get; set; } = DateTime.Now;
        public long AddedById { get; set; }
        public int? OrderById { get; set; }
        public int QuestionId { get; set; }
        public virtual FormTemplate Question { get; set; }
    }
}
