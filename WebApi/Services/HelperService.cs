using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Services
{

    public interface IHelperService
    {
        string GetDomain(string email);
        string RandomString(int size, bool lowerCase);
        string GetUploads();
        User GetUser();
        Company GetCompany();
        Task<bool> ActivateUser(string token);
        Task<bool> CheckPasswordToken(string token);
        Task<string> UpdatedPassword(string token, string password);
        Task<string> EmailiCal(int evtid);
        Task<User> AddUser(int? companyid, string name, string email, string password, string phone, string role, bool sendmail);
        string GetFilePath(string guid);
        string GetSiteUrl();
        string GetMediaPath();
        string GetMediaUrl(string guid);
        string GetUserPhoto(User usr);
        int GenAvatar(string uid, string name);
    }

    public class HelperService : IHelperService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpcontextaccessor;
        private IServiceProvider _serviceProvider;
        private RoleManager<Role> _RoleManager;
        private UserManager<User> _UserManager;
        private readonly AppSettings _appSettings;
        private readonly IEmailService _emailService;
        private readonly ICalendarService _calService;


        public HelperService (IConfiguration configuration, IOptions<AppSettings> appSettings, IServiceProvider serviceProvider, ICalendarService calService, IEmailService emailService, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _context = context;
            _httpcontextaccessor = httpContextAccessor;
            _RoleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            _UserManager = serviceProvider.GetRequiredService<UserManager<User>>();
            _calService = calService ;
            _emailService = emailService;
            _appSettings = appSettings.Value;

        }
        public string GetDomain(string email)
        {
            int indexOfAt = email.IndexOf('@');
            return email.Substring(indexOfAt + 1).ToLower();
        }
        public string GetUploads()
        {
            string folderName = "Apps\\fmr\\uploads\\";
            string dr = Directory.GetCurrentDirectory();
            string newPath = Path.Combine(Directory.GetParent(dr).Parent.ToString(), folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            return newPath;
        }
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
        public async Task<User> AddUser(int? companyid, string name, string email, string password, string phone, string role,bool sendemail)
        {
            var uguid = Guid.NewGuid();
            User usr = new User {CompanyId=companyid,UserGuid=uguid, UserName = email.ToLower(), FullName=name, Email = email, PhoneNumber = phone };

            var createUser = await _UserManager.CreateAsync(usr, password);
            if (createUser.Succeeded)
            {
                GenAvatar(uguid.ToString(), usr.FullName);
                await _UserManager.AddToRoleAsync(usr, role);
                string token = await _UserManager.GenerateEmailConfirmationTokenAsync(usr);
                var result = await _UserManager.ConfirmEmailAsync(usr,token);
                if (result.Succeeded)
                {
                    await _context.SaveChangesAsync();
                }
                return usr;
            }
            return null;
        }
        public async Task<bool> ActivateUser(string token)
        {
            bool conf = false;
            var query = await _context.UserActivate
           .Include(x => x.User)
           .Where(x => x.GuiId == token)
           .Select(x => new {id=x.Id, user= x.User, token=x.Token }).SingleOrDefaultAsync();
            if (query == null)
            {
                conf = false;
            }
            else
            {
                var result = await _UserManager.ConfirmEmailAsync(query.user, query.token);
                if(result.Succeeded)
                {
                    conf = true;
                    var UserActivate = await _context.UserActivate.FindAsync(query.id);
                    _context.UserActivate.Remove(UserActivate);
                    await _context.SaveChangesAsync();
                }
            }

            return conf;
        }
        public async Task<bool> CheckPasswordToken(string token)
        {
            var query = await _context.UserActivate
           .Include(x => x.User)
           .Where(x => x.GuiId == token)
           .Select(x => new { id = x.Id, user = x.User, token = x.Token, key = x.GuiId }).SingleOrDefaultAsync();
            if (query == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public async Task<string> UpdatedPassword(string token, string password)
        {
            string conf = "An Error occured.";
            var query = await _context.UserActivate
           .Include(x => x.User)
           .Where(x => x.GuiId == token)
           .Select(x => new { id = x.Id, user = x.User, token = x.Token }).SingleOrDefaultAsync();
            if (query == null)
            {
               
            }
            else
            {
                var result = await _UserManager.ResetPasswordAsync(query.user,query.token,password);
                if (result.Succeeded)
                {
                    var UserActivate = await _context.UserActivate.FindAsync(query.id);
                    _context.UserActivate.Remove(UserActivate);
                    await _context.SaveChangesAsync();
                    conf = "1";
                }
            }
            return conf;
        }
        public async Task<string> EmailiCal(int evtid)
        {
            var evtcal = await  _context.Event
                         .Include(x => x.Participants)
                         .Where(x => (x.Id == evtid)).SingleOrDefaultAsync();

            if (evtcal == null)
            {
                return "Not a valid event.";
            }
            var attendees = evtcal.Participants;
            if (attendees.Count <= 0)
            {
                return  "No attendees added to the event.";
            }

            string calstr = _calService.GetCalendar(evtcal);
            ContentType contentType = new ContentType("text/calendar");
            contentType.Parameters.Add("method", "REQUEST");
            contentType.Parameters.Add("component", "VEVENT");
            contentType.Parameters.Add("content-disposition", "inline;filename=meeting_" + RandomString(6, false) + ".ics");

            AlternateView calendarView = AlternateView.CreateAlternateViewFromString(calstr, contentType);
            calendarView.TransferEncoding = TransferEncoding.SevenBit;
            IList<string> mailids = new List<string>();
            IList<string> ccids = new List<string>();
            foreach (EventUser attn in attendees)
            {
                var atuser = _context.EventUser.Include(x => x.Participant).Where(x => (x.Id == attn.Id)).SingleOrDefault();
                if (!atuser.IsOrganizer)
                    mailids.Add(atuser.Participant.Email);
                else
                    ccids.Add(atuser.Participant.Email);
            }
            await  _emailService.SendEmailAsync(mailids, ccids, evtcal.Title, "Invitation from Face My Resume.", calendarView);
            return "Invite sent Successfully";
        }
        public string GetSiteUrl()
        {
            return _appSettings.SiteUrl;
        }
        public string GetRoot()
        {
            return _appSettings.RootPath;
        }
        public string GetMediaPath()
        {
            return _appSettings.MediaPath;
        }
        public string GetFilePath(string guid)
        {
            string folder = _appSettings.MediaPath+"\\"+guid;
            string dr = Directory.GetCurrentDirectory();
            string savePath = Path.Combine(Directory.GetParent(dr).Parent.ToString(), folder);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            return savePath;
        }
        public User GetUser()
        {
            User user = null;
            var userclaim = _httpcontextaccessor.HttpContext.User.Claims.Where(x => x.Type == "userid").SingleOrDefault();
            if (userclaim != null)
            {
              user = _context.User.Include(x=>x.Company).Where(x => x.Id == long.Parse(userclaim.Value)).SingleOrDefault();
            }
             //user = _context.User.Include(x=>x.Company).Where(x => x.Id == 4).SingleOrDefault();
            return user;
        }
        public string GetMediaUrl(string guid)
        {
            return _appSettings.MediaUrl+guid;
        }
        public string GetUserPhoto(User usr)
        {
            if (usr == null)
               return _appSettings.MediaUrl + "00000000-0000-0000-0000-000000000000/profimg.png";
            else
                return _appSettings.MediaUrl + usr.UserGuid + "/profimg.png";
        }
        public Company GetCompany()
        {
            return GetUser().Company;
        }
        private List<string> _BackgroundColours = new List<string> { "00AA55", "009FD4", "B381B3", "939393", "E3BC00", "D47500", "DC2A2A", "339966", "CC33FF", "FF5050" };
        public int GenAvatar(string uid, string name)
        {
            
            int i = 0;
            var avatarString = string.Format("{0}{1}", name[0], name[1]).ToUpper();
            var randomIndex = new Random().Next(0, _BackgroundColours.Count - 1);
            var bgColour = _BackgroundColours[randomIndex];
            var bmp = new Bitmap(192, 192);
            var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            var font = new Font("Arial", 72, FontStyle.Bold, GraphicsUnit.Pixel);
            var graphics = Graphics.FromImage(bmp);
            graphics.Clear((Color)new ColorConverter().ConvertFromString("#" + bgColour));
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.DrawString(avatarString, font, new SolidBrush(Color.WhiteSmoke), new RectangleF(0, 0, 192, 192), sf);
            graphics.Flush();
            // bmp.Save();

                using (var memStream = new System.IO.MemoryStream())
            {
                //bmp.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                string savepth = GetFilePath(uid) + "\\profimg.png";
                //Image img = System.Drawing.Image.FromStream(memStream);
                bmp.Save(savepth, ImageFormat.Png);
                memStream.Dispose();
                bmp.Dispose();
               // img.Dispose();

                i = 1;
            }

            return i;

           
        }
    }
}
