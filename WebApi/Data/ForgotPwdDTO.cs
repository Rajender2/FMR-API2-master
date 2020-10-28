using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Data
{
    public class ForgotPwdDTO
    {
        public string Email { get; set; }
    }
    public class ResetDTO
    {
        public string token { get; set; }
        public string password { get; set; }
    }
}
