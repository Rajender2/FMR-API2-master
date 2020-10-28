using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class AppSettings
    {
        public string RootPath { get; set; }
        public string MediaPath { get; set; }
        public string MediaUrl { get; set; }
        public string SiteUrl { get; set; }
    }
}
