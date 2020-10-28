using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Services;
using WebApi.Data;
using WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace WebApi.Controllers
{
    [Route("[controller]/[Action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AccountService _accountService;
        private RoleManager<Role> _RoleManager;
        private UserManager<User> _UserManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IHelperService _helperService;
        public AccountController(DataContext context,AccountService accountService,IHelperService helperService, IEmailService emailService, IServiceProvider serviceProvider, SignInManager<User> signInManager)
        {
            _accountService = accountService;
            _context = context;
            _RoleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            _UserManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _signInManager = signInManager;
            _emailService = emailService;
            _helperService = helperService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterDTO usr)
        {
            if (!_context.User.Any(u => u.Email == usr.Email))
            {
                var uguid = Guid.NewGuid();
                var user = new User
                {
                    UserName = usr.Email.ToLower(),
                    Email = usr.Email,
                    FullName = usr.Name,
                    PasswordHash = usr.Password,
                    UserGuid = uguid,
                };
               
                if (usr.Type == 2)
                {
                    var comp = new Company();
                    string emaildom = _helperService.GetDomain(usr.Email);
                    if (!_context.User.Any(x=>x.Email.Contains(emaildom)))
                    { 
                        var addr = new Address();
                        addr.Phone = usr.Phone;
                        comp.Name = usr.Company;
                        comp.UID = Guid.NewGuid();
                        comp.Address = addr;
                        user.Company = comp;
                        comp.Created = DateTime.Now;
                        comp.Updated = DateTime.Now;
                    }
                    else
                    {
                        return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status400BadRequest, Message = "Company already exists, contact administrator" });
                    }
                }

                var createUser = await _UserManager.CreateAsync(user, usr.Password);
                if (createUser.Succeeded)
                {
                    if (usr.Type == 2)
                    {
                        await _UserManager.AddToRoleAsync(user, "ADMIN");
                    }
                    else
                    {
                        await _UserManager.AddToRoleAsync(user, "CANDIDATE");
                       
                    }
                    //Gen Avatar
                    _helperService.GenAvatar(uguid.ToString(), user.FullName);

                    if (!string.IsNullOrEmpty(usr.Token)&& usr.Type == 4)
                    {

                        if (usr.Token.Contains("-"))
                        {
                            //Find JD and select candidate by user email and company id
                            var jdinfo = await _context.JobOrder.Where(x => (x.InviteId == usr.Token)).FirstOrDefaultAsync();
                            //If new add candidate and select id
                            if (jdinfo != null)
                            {
                                var cand = await _context.Candidate.Where(c => (c.Email == usr.Email && c.CompanyId == jdinfo.CompanyId)).FirstOrDefaultAsync();
                                _context.Entry<JobOrder>(jdinfo).State = EntityState.Detached;

                                Candidate cnew = new Candidate();
                                if (cand == null)
                                {
                                    cnew.Name = user.FullName;
                                    cnew.UserId = user.Id;
                                    cnew.Email = user.Email;
                                    cnew.CompanyId = jdinfo.CompanyId;
                                    cnew.Created = DateTime.Now;
                                    await _context.Candidate.AddAsync(cnew);
                                    
                                }
                                else
                                {
                                    _context.Entry<Candidate>(cand).State = EntityState.Detached;
                                    cnew.Id = cand.Id;
                                    _context.Candidate.Attach(cnew);
                                    cnew.Name = user.FullName;
                                    cnew.UserId = user.Id;
                                }
                                await _context.SaveChangesAsync();
                                var jdcand = await _context.JobCandidate.Where(x => (x.jobOrderId == jdinfo.Id && x.CandidateId== cnew.Id)).FirstOrDefaultAsync();

                                JobCandidate jc = new JobCandidate();
                                if (jdcand == null)
                                {
                                   
                                    jc.CandidateId = cnew.Id;
                                    jc.jobOrderId = jdinfo.Id;
                                    jc.AddedOn = DateTime.Now;
                                    jc.AddedById = cnew.Id;
                                    await _context.JobCandidate.AddAsync(jc);
                                    
                                }
                                else
                                {
                                    _context.Entry<JobCandidate>(jdcand).State = EntityState.Detached;
                                    _context.JobCandidate.Attach(jc);
                                    jc.CandidateId = cnew.Id;
                                    jc.jobOrderId = jdinfo.Id;
                                    jc.AddedOn = DateTime.Now;
                                    jc.AddedById = cnew.Id;

                                }
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            InviteCandidate candid = await _context.InviteCandidate.Where(i => i.Token == usr.Token).SingleOrDefaultAsync();
                            if (candid == null)
                            {

                            }
                            else
                            {
                                Candidate cnew = new Candidate();
                                cnew.Id = candid.CandidateId;
                                _context.Candidate.Attach(cnew);
                                cnew.UserId = user.Id;
                                _context.InviteCandidate.Remove(candid);
                            }
                         
                        }
                        string token = await _UserManager.GenerateEmailConfirmationTokenAsync(user);
                        var result = await _UserManager.ConfirmEmailAsync(user, token);
                        if (result.Succeeded)
                        {
                            await _context.SaveChangesAsync();
                        }

                    }
                    else
                    {

                        string token = await _UserManager.GenerateEmailConfirmationTokenAsync(user);
                        string keyval = Guid.NewGuid().ToString("N");
                        UserActivate usract = new UserActivate { GuiId = keyval, Token = token, UserId = user.Id };
                        _context.UserActivate.Add(usract);
                        await _context.SaveChangesAsync();
                        await SendActivation(user, keyval);

                    }
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="Account created successfully, Please check mail and activate account" });
                }
                else
                {
                    return BadRequest(new ErrorDto{ StatusCode=StatusCodes.Status400BadRequest, Message="Password policy not met" });
                }

            }
            else
            {
                return BadRequest(new ErrorDto{ StatusCode = StatusCodes.Status400BadRequest, Message= "Email id already exists" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {

            //var user = _context.User.Include(x => x.Manufacturer).Include(x => x.Roles).Where(x => x.UserName == loginDto.UserName && x.PasswordHash == loginDto.Password).SingleOrDefault();
            var loginres = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, true, lockoutOnFailure: false);
            if (loginres.Succeeded)
            {
                //  var user = _context.User.Include(x => x.Manufacturer).Include(x => x.UserRole).Where(x => x.UserName == loginDto.UserName).SingleOrDefault();
                var user = _context.User.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).Where(x => x.UserName == loginDto.Email).SingleOrDefault();
                if(!string.IsNullOrEmpty(loginDto.Token))
                {
                    if (loginDto.Token.Contains("-"))
                    {
                        //Find JD and select candidate by user email and company id
                        var jdinfo = await _context.JobOrder.Where(x => (x.InviteId == loginDto.Token)).FirstOrDefaultAsync();
                        //If new add candidate and select id
                        if (jdinfo != null)
                        {
                            var cand = await _context.Candidate.Where(c => (c.Email == loginDto.Email && c.CompanyId == jdinfo.CompanyId)).FirstOrDefaultAsync();
                            _context.Entry<JobOrder>(jdinfo).State = EntityState.Detached;

                            Candidate cnew = new Candidate();
                            if (cand == null)
                            {
                                cnew.Name = user.FullName;
                                cnew.UserId = user.Id;
                                cnew.Email = user.Email;
                                cnew.CompanyId = jdinfo.CompanyId;
                                cnew.Created = DateTime.Now;
                                await _context.Candidate.AddAsync(cnew);

                            }
                            else
                            {
                                _context.Entry<Candidate>(cand).State = EntityState.Detached;
                                cnew.Id = cand.Id;
                                _context.Candidate.Attach(cnew);
                                cnew.Name = user.FullName;
                                cnew.UserId = user.Id;
                            }

                            var jdcand = await _context.JobCandidate.Where(x => (x.jobOrderId == jdinfo.Id && x.CandidateId == cnew.Id)).FirstOrDefaultAsync();

                            JobCandidate jc = new JobCandidate();
                            if (jdcand == null)
                            {

                                jc.CandidateId = cnew.Id;
                                jc.jobOrderId = jdinfo.Id;
                                jc.AddedOn = DateTime.Now;
                                jc.AddedById = cnew.Id;
                                await _context.JobCandidate.AddAsync(jc);

                            }
                            //else
                            //{
                            //    _context.Entry<JobCandidate>(jdcand).State = EntityState.Detached;
                            //    _context.JobCandidate.Attach(jc);
                            //    jc.CandidateId = cnew.Id;
                            //    jc.jobOrderId = jdinfo.Id;
                            //}
                            await _context.SaveChangesAsync();
                        }

                    }
                    else
                    {
                        InviteCandidate candid = await _context.InviteCandidate.Where(i => i.Token == loginDto.Token).SingleOrDefaultAsync();
                        if (candid == null)
                        {

                        }
                        else
                        {
                            Candidate cnew = new Candidate();
                            cnew.Id = candid.CandidateId;
                            _context.Candidate.Attach(cnew);
                            cnew.UserId = user.Id;
                            _context.InviteCandidate.Remove(candid);

                            await _context.SaveChangesAsync();
                        }
                    }
                }
                var jwtToken = _accountService.Login(user);
                if (jwtToken == null)
                {
                      return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Invalid User Info Detected" });
                }
                var result = new Dictionary<string, object>();
                result.Add("authtoken", jwtToken);
                result.Add("userid", user.Id);
                result.Add("fullname", user.FullName);
                result.Add("email", user.Email);
                result.Add("companyid", user.CompanyId);
                result.Add("photo", _helperService.GetUserPhoto(user));
                var roles = "";
                foreach (UserRole ur in user.UserRoles)
                {
                    roles = String.Join(",", ur.Role.Name);
                }
                result.Add("Role", roles);

                //result.Add("User", user);
                //return Ok(token);
                return Ok(new { StatusCode = StatusCodes.Status200OK, Result = result });
            }
            else
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Invalid username or password" });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePwdDTO pwd)
        {
                User user = _helperService.GetUser();
                if (user == null)
                {
                    return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
                }
                else
                {
                    var pwdstr = await _UserManager.ChangePasswordAsync(user, pwd.OldPassword, pwd.NewPassword);
                    if(pwdstr.Succeeded)
                        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Password changed successfully." });
                    else
                    return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = pwdstr.Errors.Select(x=> x.Description).SingleOrDefault()});
                }
        }

        [HttpGet]
        public async Task<ActionResult> VerifyEmail(string token)
        {
            string strconfrim = "Verification code expired or invalid.";
            bool conf = await _helperService.ActivateUser(token);
            if (conf)
            {
                strconfrim = "Email verified successfully.";
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = strconfrim });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPwdDTO pwd)
        {
            if (ModelState.IsValid)
            {
                var user = await _UserManager.FindByEmailAsync(pwd.Email);
                if (user == null || !(await _UserManager.IsEmailConfirmedAsync(user)))
                {

                }
                else
                {
                    string token = await _UserManager.GeneratePasswordResetTokenAsync(user);
                    string keyval = Guid.NewGuid().ToString("N");
                    UserActivate usract = new UserActivate { GuiId = keyval, Token = token, UserId = user.Id };
                    _context.UserActivate.Add(usract);
                    await _context.SaveChangesAsync();
                    await SendRestMail(user,keyval);
                }
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "We will email you the password reset link if the email entered is correct" });
        }

        [HttpGet]
        public async Task<ActionResult> PasswordReset(string token)
        {
            bool key = await _helperService.CheckPasswordToken(token);
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = key});
        }

        [HttpPost]
        public async Task<ActionResult> CreatePassword([FromBody]ResetDTO verify)
        {
            string key = await _helperService.UpdatedPassword(verify.token, verify.password);

            if(key=="1")
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Password changed successfully." });
            else
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status400BadRequest, Message = "Token invalid or expired." });
        }

        private async Task SendActivation(User user, string token)
        {
            try
            {
                IList<string> lstEmail = new List<string> { user.Email };

                string lnkTxt = _helperService.GetSiteUrl() + "activation?token="+token;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>Face My Resume</title>");
                sb.AppendLine("<style type=\"text/css\"> body {font-family: \"Lato\", \"Lucida Grande\", \"Lucida Sans Unicode\", Tahoma, Sans-Serif; font-size:18px;}</style>");
                sb.AppendLine("</head><body><div style=\"text-align:center\"><p><strong>Hi "+user.FullName+",</strong></p>");
                sb.AppendLine("<p>Thank you for signing up for Face My Resume.  To confirm your account please click the activation button below.</p>");
                sb.AppendLine("<p><a href=\""+ lnkTxt + "\" style=\"background:#d01013; padding:10px 20px; color:#fff; text-decoration:none; font-size:18px; font-weight: 600\">Activate Account</a></p>");
                sb.AppendLine("<p>Paste the link in your browser if the button is not working.</p><p>"+ lnkTxt + "</p>");
                sb.AppendLine("<p><strong>The FMR Team</strong><br><a href=\"https://www.facemyresume.com\">www.facemyresume.com</a></p></div></body></html>");
                await _emailService.SendEmailAsync(lstEmail, null, "Account Activation - Face My Resume", sb.ToString());
            }
            catch
            {

            }

        }

        private async Task SendRestMail(User user, string token)
        {
            try
            {
                IList<string> lstEmail = new List<string> { user.Email };
                string lnkTxt = _helperService.GetSiteUrl() + "reset?token=" + token;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>Face My Resume</title>");
                sb.AppendLine("<style type=\"text/css\"> body {font-family: \"Lato\", \"Lucida Grande\", \"Lucida Sans Unicode\", Tahoma, Sans-Serif; font-siz:18px;}</style>");
                sb.AppendLine("</head><body><div style=\"text-align:center\"><p><strong>Hi " + user.FullName +",</strong></p>");
                sb.AppendLine("<p>A request to reset your Face My Resume password has been submitted. Please click the button below to reset your password.</p>");
                sb.AppendLine("<p><a href=\""+lnkTxt+"\" style=\"background:#d01013; padding:10px 20px; color:#fff; text-decoration:none; font-size:18px; font-weight: 600\">Reset Password</a></p>");
                sb.AppendLine("<p>Paste the link in your browser if the button is not working.</p><p>" + lnkTxt + "</p>");
                sb.AppendLine("<p>If you did not make this request, ignore this email.</p>");
                sb.AppendLine("<p><strong>The FMR Team</strong><br><a href=\"https://www.facemyresume.com\">www.facemyresume.com</a></p></div></body></html>");

                await _emailService.SendEmailAsync(lstEmail , null, "Reset Password - Face My Resume",sb.ToString());
            }
            catch
            {

            }
        }

    }
}