using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using WebApi.Data;
using WebApi.Services;
using WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace WebApi.Controllers
{
    [Route("[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class CommonController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHelperService _helperService;

        public CommonController(DataContext context, IHelperService helperService)
        {
            _context = context;
            _helperService = helperService;
        }

        [HttpGet]
        public async Task<IActionResult> CalendarEvents([FromQuery]calevent calparams)
        {
            User usr = _helperService.GetUser();
            if (usr == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                DateTime dtstart = DateTime.ParseExact(calparams.start, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                DateTime dtend = DateTime.ParseExact(calparams.end, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                var query = await _context.Event
               .Join(_context.EventUser, e => e.Id, u => u.EventId, (e, u) => new { e, u })
                .Where(x => (x.u.ParticipantId == usr.Id) && ((x.e.StartTime >= dtstart) && (x.e.StartTime <= dtend)))
               .Select(x => new
               {
                   id = x.e.Id,
                   start = x.e.StartTime,
                   end = x.e.EndTime,
                   title = x.e.Title,
                   description = x.e.Description,
                   calendarid = x.e.CalendarId,
                   participants = x.e.Participants.Where(p => p.EventId == x.e.Id)
                               .Select(p => new
                               {
                                   id = p.ParticipantId,
                                   name = p.Participant.FullName,
                                   email = p.Participant.Email,
                                   status = p.Status,
                                   photo = _helperService.GetUserPhoto(p.Participant),
                                   organizer = p.IsOrganizer
                               }).ToList()
               }).ToListAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEvent(int EventId)
        {
            User usr = _helperService.GetUser();
            if (usr == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.Event
               .Include(x => x.Participants)
               .Where(x => (x.Id == EventId))
             .Select(x => new
             {
                 id = x.Id,
                 starttime = x.StartTime,
                 endtime = x.EndTime,
                 subject = x.Title,
                 description = x.Description,
                 updatedon = x.UpdatedOn,
                 calendarid = x.CalendarId,
                 eventtype = x.EventType,
                 location = x.Location,
                 eventstatus = x.Status,
                 participants = x.Participants
                 .Select(p => new
                 {
                     id = p.Id,
                     name = p.Participant.FullName,
                     email = p.Participant.Email,
                     photo = _helperService.GetUserPhoto(p.Participant),
                     status = p.Status,
                     organizer = p.IsOrganizer
                 }).ToList(),
             }).SingleOrDefaultAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveEvent([FromBody]evnt edata)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var Eventmodel = await _context.Event.Include(x => x.Calendar).Where(x => x.Id == edata.Id).SingleOrDefaultAsync();
                if (Eventmodel != null)
                {
                    _context.Entry<Event>(Eventmodel).State = EntityState.Detached;
                }
                Event evt = new Event();
                if (edata.Id == 0)
                {
                    Calendar caln = new Calendar();
                    caln.AddedOn = DateTime.Now;
                   await _context.Calendar.AddAsync(caln);
                    evt.Title = edata.Title;
                    evt.StartTime = edata.Start;
                    evt.EndTime = edata.End;
                    evt.Description = edata.Description;
                    evt.Status = 0;
                    evt.CalendarId = caln.Id;
                    evt.UID = Guid.NewGuid().ToString();
                    evt.Companyid = user.CompanyId;
                    evt.EventType = 1;
                    await _context.AddAsync(evt);
                    List<EventUser> plist = new List<EventUser>();
                    foreach (long pid in edata.Participantids.Distinct().ToArray())
                    {
                        EventUser puser = new EventUser();
                        puser.ParticipantId = pid;
                        puser.Status = 0;
                        puser.EventId = evt.Id;
                        if (pid == user.Id)
                        {
                            puser.IsOrganizer = true;
                        }
                        plist.Add(puser);
                    }
                   
                    _context.AddRange(plist);
                }
                else
                {
                    evt.Id = edata.Id;
                    _context.Event.Attach(evt);
                    evt.Title = edata.Title;
                    evt.StartTime = edata.Start;
                    evt.EndTime = edata.End;
                    evt.Description = edata.Description;
                    evt.Status = edata.Status;
                    List<EventUser> plist = new List<EventUser>();
                    foreach (long pid in edata.Participantids.Distinct().ToArray())
                    {
                        EventUser puser = new EventUser();
                        puser.EventId = evt.Id;
                        puser.ParticipantId = pid;
                        puser.Status = 0;
                        if (pid == user.Id)
                        {
                            puser.IsOrganizer = true;
                        }
                        plist.Add(puser);
                    }
                    var model = _context.Event
                   .Include(x => x.Participants)
                   .FirstOrDefault(x => x.Id == evt.Id);
                    _context.TryUpdateManyToMany(model.Participants, plist
                                    .Select(x => new EventUser
                                    {
                                        ParticipantId = x.ParticipantId,
                                        EventId = evt.Id,
                                        IsOrganizer = x.IsOrganizer,
                                        Status = x.Status
                                    }), x => x.ParticipantId, true);

                }
                await _context.SaveChangesAsync();

                return await GetEvent(evt.Id);
            }
        }

        [HttpGet]
        public IActionResult Profile()
        {
            User usr = _helperService.GetUser();
            if (usr == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                var cusr = new Dictionary<string, object>();
                cusr.Add("fullname", usr.FullName);
                cusr.Add("email", usr.Email);
                cusr.Add("phone", usr.PhoneNumber);
                if (usr.DefaultAddressId != null)
                {
                    var addr = _context.Address.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.id == usr.DefaultAddressId).SingleOrDefault();
                    cusr.Add("address", addr.AddressLine);
                    cusr.Add("phone2", addr.Phone);
                    cusr.Add("zipcode", addr.ZipCode);
                    if (addr.City != null)
                        cusr.Add("city", addr.CityId);
                    cusr.Add("state", addr.City.State.id);

                }

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = cusr });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveProfile(profile userdata)
        {
            User usr = _helperService.GetUser();
            if (usr == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {


                Address addrr = new Address();
                if (usr.DefaultAddressId == null)
                {
                    addrr.AddressLine = userdata.address;
                    if (userdata.city != null || userdata.city != 0)
                        addrr.CityId = userdata.city;
                    addrr.Phone = userdata.phone2;
                    addrr.ZipCode = userdata.zipcode;

                    _context.Address.Add(addrr);
                    usr.DefaultAddress = addrr;
                }
                else
                {

                    addrr.id = usr.DefaultAddressId.GetValueOrDefault();
                    _context.Address.Attach(addrr);
                    addrr.AddressLine = userdata.address;
                    if (userdata.city != null || userdata.city != 0)
                        addrr.CityId = userdata.city;
                    addrr.Phone = userdata.phone2;
                    addrr.ZipCode = userdata.zipcode;
                }
                await _context.SaveChangesAsync();

                usr.FullName = userdata.fullname;
                usr.PhoneNumber = userdata.phone;

                await _context.SaveChangesAsync();
                return Profile();
            }
        }

        [HttpPost]
        public ActionResult ProfilePhoto(IFormFile file)
        {
            User usr = _helperService.GetUser();
            if (usr == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                try
                {
                    string fileName = "profimg.png";
                    if (file.Length > 0)
                    {
                        //fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Extension.ToLower() == ".jpg" || fi.Extension.ToLower() == ".png" || fi.Extension.ToLower() == ".bmp" || fi.Extension.ToLower() == ".gif")
                        {
                            string fileN = "profimg.png";
                            string folder = _helperService.GetFilePath(usr.UserGuid.ToString());
                            string dr = Directory.GetCurrentDirectory();
                            string savePath = Path.Combine(Directory.GetParent(dr).Parent.ToString(), folder);
                            if (!Directory.Exists(savePath))
                            {
                                Directory.CreateDirectory(savePath);
                            }
                            string fullPath = Path.Combine(savePath, fileN);

                            Image image = Image.FromStream(file.OpenReadStream(), true, true);
                            var newImage = new Bitmap(300, 300);
                            using (var g = Graphics.FromImage(newImage))
                            {
                                g.DrawImage(image, 0, 0, 300, 300);
                                newImage.Save(fullPath, ImageFormat.Png);
                            }

                        }
                        else
                        {
                            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Unsuported file format.", result = fileName });
                        }
                    }
                    else
                    {
                        _helperService.GenAvatar(usr.UserGuid.ToString(), usr.FullName);
                        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Remove Successfull.", result = fileName });

                    }
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Update Successfull.", result = fileName });
                }
                catch (System.Exception ex)
                {
                    return Ok(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
                }
            }

        }

        [HttpGet]
        public async Task<IActionResult> ActivityLog([FromQuery]Paging pg)
        {
            User usr = _helperService.GetUser();
            if (usr == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.ActivityLog
                 .Where(x => x.UserId == usr.Id)
                 .Select(x => new
                 {
                     id=x.Id,
                     activity = x.Acitvity,
                     addedon = x.AddedOn
                 });
                query = pg.q != "" ? query.Where(x => (x.activity.Contains(pg.q)) || (x.activity.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id); ;

                if (pg.size > 100) { pg.size = 25; }
                var qdata = await query.ToPagedListAsync(pg.page, pg.size);
                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Notifications([FromQuery]Paging pg)
        {
            User usr = _helperService.GetUser();
            if (usr == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = "";

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "" });
            }
        }

    }
}