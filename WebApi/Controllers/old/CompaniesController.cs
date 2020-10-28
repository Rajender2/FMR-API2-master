using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;
namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly DataContext _context;
        private IServiceProvider _serviceProvider;
        private RoleManager<Role> _RoleManager;
        private UserManager<User> _UserManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        public CompanyController(DataContext context, IEmailService emailService, IServiceProvider serviceProvider, SignInManager<User> signInManager)
        {
            _context = context;
            _RoleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            _UserManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompany()
        {
            var companies = await _context.Company.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = companies });
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            var company = await _context.Company.Include(x => x.Address).Where(x => x.Id == id).SingleOrDefaultAsync();

            if (company == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = company });
        }

        // PUT: api/Companies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(int id, Company company)
        {
            if (id != company.Id)
            {
                return BadRequest();
            }

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = company });
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Company>> Patch(int id, [FromBody]JsonPatchDocument<Company> Company)
        {
            var CompanyDB = await _context.Company.FindAsync(id);
            Company.ApplyTo(CompanyDB);
            return CompanyDB;
        }
     

        // POST: api/Companies
        [HttpPost]
        public async Task<ActionResult<Company>> PostCompany(Company company)
        {
            _context.Company.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompany", new { id = company.Id }, company);
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Company>> DeleteCompany(int id)
        {
            var company = await _context.Company.FindAsync(id);
            if (company == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.Company.Remove(company);
            await _context.SaveChangesAsync();

            return company;
        }

        private bool CompanyExists(int id)
        {
            return _context.Company.Any(e => e.Id == id);
        }


        [Route("AddUser")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<User>>> AddUser(UserDTO usr)
        {
            User admusr = GetUser();
            if (admusr != null)
            {
                if (!_context.User.Any(u => u.Email == usr.Email))
                {
                    if (usr.Type != 0)
                    {
                        var user = new User
                        {
                            UserName = usr.Email.ToLower(),
                            Email = usr.Email,
                            FullName = usr.Name,
                            PasswordHash = usr.Password,
                            CompanyId = usr.CompanyId
                        };
                        var createUser = await _UserManager.CreateAsync(user, usr.Password);
                        if (createUser.Succeeded)
                        {
                            if (usr.Type == 2)
                                await _UserManager.AddToRoleAsync(user, "ADMIN");
                            else if (usr.Type == 3)
                                await _UserManager.AddToRoleAsync(user, "MANAGER");
                            string token = await _UserManager.GenerateEmailConfirmationTokenAsync(user);

                            // await _emailService.SendEmailAsync(user.Email, "Confirm your account", $"Please confirm your account by clicking this link: {token} ");
                            string invite = admusr.FullName + ", Invited to join Face My Resume app, use your email and temp password to login, " + usr.Password;
                            return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "User Created Successfully, An Invitation sent to email. " + invite, id = user.Id, user });
                        }
                        else
                        {
                            return Ok(new ErrorDto { StatusCode = StatusCodes.Status400BadRequest, Message = "Password policy not met" });
                        }
                    }
                    else
                    {
                        return Ok(new ErrorDto { StatusCode = StatusCodes.Status400BadRequest, Message = "User role not selected." });
                    }
                }
                else
                {
                    return Ok(new ErrorDto { StatusCode = StatusCodes.Status409Conflict, Message = "Email already exisits" });
                }

            }
            return Ok(new { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
        }


        [Route("Users")]
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<vwUser>>> Users()
        {
            User usr = GetUser();
            if (usr != null && usr.CompanyId!=null)
            {
                var list = await _context.vwUser.Where(x => x.CompanyId == usr.CompanyId).ToListAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = list });
            }
            return Ok(new { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
        }

        private User GetUser()
        {
            User user = null;
            var userclaim = HttpContext.User.Claims.Where(x => x.Type == "userid").SingleOrDefault();
            if (userclaim != null)
            {
                user = _context.User.Where(x => x.Id == long.Parse(userclaim.Value)).SingleOrDefault();
            }
            return user;

        }
    }
}
