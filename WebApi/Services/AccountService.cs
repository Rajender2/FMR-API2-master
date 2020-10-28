using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Services
{
    public class AccountService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public AccountService(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string Login(User user)
        {
            //var user = _users.Where(x => x.Username == loginDto.Username && x.Password == loginDto.Password).SingleOrDefault();

            //       var roles = _context.UserRoles.Where(x => x.UserId == user.Id);
            var signingKey = Convert.FromBase64String(_configuration["Jwt:SigningSecret"]);
            var expiryDuration = int.Parse(_configuration["Jwt:ExpiryDuration"]);
            var roles = "";
            foreach (UserRole ur in user.UserRoles)
            {
                roles = String.Join(",", ur.Role.Name);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = null,              // Not required as no third-party is involved
                Audience = null,            // Not required as no third-party is involved
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expiryDuration),
                Subject = new ClaimsIdentity(new List<Claim> {
                        new Claim("userid", user.Id.ToString()),
                        new Claim(ClaimTypes.Role,roles)
                    }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var token = jwtTokenHandler.WriteToken(jwtToken);
            return token;
        }

    }
}
