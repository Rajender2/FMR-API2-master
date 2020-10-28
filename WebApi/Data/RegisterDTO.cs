using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Data
{
    public class RegisterDTO
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string Token { get; set; }

    }
}
