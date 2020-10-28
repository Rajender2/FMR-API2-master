using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinyCsvParser;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]/[Action]")]
    [ApiController]
    public class UtilsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;
        private readonly ICalendarService _calService;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IHelperService _helperService;
        public UtilsController(DataContext context, IHelperService helperService, ICalendarService calService, IEmailService emailService, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _calService = calService;
            _emailService = emailService;
            _hostingEnvironment = hostingEnvironment;
            _helperService = helperService;
        }

        // GET: api/Utils/5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<State>>> States()
        {
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = await _context.State.ToListAsync() });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> Cities(int stateid)
        {
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = await _context.City.Where(x => x.State.id == stateid).ToListAsync() });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobType>>> JobTypes()
        {
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = await _context.Jobtype.ToListAsync() });
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentStatus>>> AssessmentStatus()
        {
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = await _context.AssessmentStatus.ToListAsync() });
        }

        [HttpGet]
        public ActionResult DownloadiCal()
        {
            string iCal = _calService.GetCalendar();
            byte[] calBytes = System.Text.Encoding.UTF8.GetBytes(iCal);

            DateTime dtnow = DateTime.Now;
            string strnow = toUniversalTime(DateTime.Now);
            string dtstart = toUniversalTime(dtnow.AddDays(1));
            string dtend = toUniversalTime(dtnow.AddDays(1).AddMinutes(30));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:REQUEST");
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine("TZID:Eastern Standard Time");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine("DTSTART:"+ dtstart);
            sb.AppendLine("RRULE:FREQ=YEARLY;BYDAY=1SU;BYHOUR=2;BYMINUTE=0;BYMONTH=11");
            sb.AppendLine("TZNAME:Eastern Standard Time");
            sb.AppendLine("TZOFFSETFROM:-0400");
            sb.AppendLine("TZOFFSETTO:-0500");
            sb.AppendLine("END:STANDARD");
            sb.AppendLine("BEGIN:DAYLIGHT");
            sb.AppendLine("DTSTART:"+ dtstart);
            sb.AppendLine("RRULE:FREQ=YEARLY;BYDAY=2SU;BYHOUR=2;BYMINUTE=0;BYMONTH=3");
            sb.AppendLine("TZNAME:Eastern Daylight Time");
            sb.AppendLine("TZOFFSETFROM:-0500");
            sb.AppendLine("TZOFFSETTO:-0400");
            sb.AppendLine("END:DAYLIGHT");
            sb.AppendLine("END:VTIMEZONE");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("ATTENDEE;CN=John Doe;ROLE=REQ-PARTICIPANT;PARTSTAT=TENTATIVE;CUTYPE=INDIVIDUAL;RSVP=TRUE:MAILTO:john.doe@outlook.com");
            sb.AppendLine("DESCRIPTION:");
            sb.AppendLine("DTEND;TZID=Eastern Standard Time:"+ dtend);
            sb.AppendLine("DTSTAMP:"+ strnow);
            sb.AppendLine("DTSTART;TZID=Eastern Standard Time:"+ dtstart);
            sb.AppendLine("LAST-MODIFIED:"+ strnow);
            sb.AppendLine("LOCATION:");
            sb.AppendLine("ORGANIZER;CN=Face My Resume:MAILTO:noreply@lsinextgen.com");
            sb.AppendLine("SEQUENCE:0");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("SUMMARY:Nevermind");
            sb.AppendLine("UID:" + Guid.NewGuid().ToString());
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");
            byte[] sbCal = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(sbCal, "text/calendar", "event"+ _helperService.RandomString(6,false) +".ics");
        }

        [HttpGet]
        public ActionResult EmailiCal(int evtid)
        {
            var evtcal = _context.Event
                         .Include(x => x.Participants)
                         .Where(x => (x.Id == evtid)).SingleOrDefault();

            if (evtcal == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Not a valid event." });
            }
            var attendees = evtcal.Participants;
            if (attendees.Count <= 0)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "No attendees added to the event." });
            }

            string calstr = _calService.GetCalendar(evtcal);
            ContentType contentType = new ContentType("text/calendar");
            contentType.Parameters.Add("method", "REQUEST");
            contentType.Parameters.Add("component", "VEVENT");
            contentType.Parameters.Add("content-disposition", "inline;filename=meeting_" + _helperService.RandomString(6, false) + ".ics");

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
            _emailService.SendEmailAsync(mailids,ccids, evtcal.Title, "Invitation sent using Face My Resume.",calendarView);
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Mail sent Successfully" });
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                //string foldpath = _helperService.RandomString(16, true);
                //string folderName = "..\\..\\Apps\\fmr\\Media\\"+ foldpath;
                //string dr = Directory.GetCurrentDirectory();
                //string newPath = Path.Combine(dr, folderName);
                //if (!Directory.Exists(newPath))
                //{
                //    Directory.CreateDirectory(newPath);
                //}

                string fileName = "";
                fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                FileInfo fi = new FileInfo(fileName);
                if (fi.Extension.ToLower() == ".webm")
                {
                    string fileN = _helperService.RandomString(8, true) + fi.Extension;
                    string folder = _helperService.GetMediaPath() + "\\temp";
                    string dr = Directory.GetCurrentDirectory();
                    string savePath = Path.Combine(Directory.GetParent(dr).Parent.ToString(), folder);
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    string fullPath = Path.Combine(savePath, fileN);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = "File Uploded Successfully", result = fullPath });
                }
               
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "invalid file format" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
            }
        }
        private List<string> _BackgroundColours = new List<string> { "00AA55", "009FD4", "B381B3", "939393", "E3BC00", "D47500", "DC2A2A", "339966", "CC33FF", "FF5050" };

        [HttpGet]
        public async Task<IActionResult> Invite(string token)
        {

            if (token.Contains("-"))
            {
                var inv = await _context.JobOrder.Where(x => x.InviteId == token)
                      .Select(j => new
                      {
                          Id = j.Id,
                          jobtitle = j.Title,
                          jobtypeid = j.JobTypeId,
                          jobtype = j.JobType.Type,
                          skills = j.Skills,
                          openings = j.Openings,
                          experience = j.Experience,
                          company = j.Company,
                            //location = j.JobOrder.Company.Address.City.Name + ", " + j.JobOrder.Company.Address.City.State.Code,
                            location = j.Location,
                          companyname = j.CompanyName,
                          published = j.Published,
                          summary = j.Summary,
                          notes = j.Notes
                      })
                      .SingleOrDefaultAsync();
                if (inv == null)
                {
                    return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Invitation Link is Invalid or expired." });
                }
                else
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, result = inv });
                }

            }
            else
            {
                var inv = await _context.InviteCandidate.Include(j => j.JobOrder).Where(x => x.Token == token)
                      .Select(j => new
                      {
                          Id = j.JobOrder.Id,
                          jobtitle = j.JobOrder.Title,
                          jobtypeid = j.JobOrder.JobTypeId,
                          jobtype = j.JobOrder.JobType.Type,
                          skills = j.JobOrder.Skills,
                          openings = j.JobOrder.Openings,
                          experience = j.JobOrder.Experience,
                          company = j.JobOrder.Company,
                            //location = j.JobOrder.Company.Address.City.Name + ", " + j.JobOrder.Company.Address.City.State.Code,
                            location = j.JobOrder.Location,
                          companyname = j.JobOrder.CompanyName,
                          published = j.JobOrder.Published,
                          summary = j.JobOrder.Summary,
                          notes = j.JobOrder.Notes
                      })
                      .SingleOrDefaultAsync();
                if (inv == null)
                {
                    return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Invitation Link is Invalid or expired." });
                }
                else
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, result = inv });
                }
            }
        }

        [HttpGet]
        public ActionResult Avatar(string firstName, string lastName)
        {

          //  var avatarString = string.Format("{0}{1}", firstName[0], lastName[0]).ToUpper();
          var avatarString = string.Format("{0}", firstName[0]).ToUpper();
            var randomIndex = new Random().Next(0, _BackgroundColours.Count - 1);
            var bgColour = _BackgroundColours[randomIndex];

            var bmp = new Bitmap(192, 192);
            var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            var font = new Font("Arial", 80, FontStyle.Bold, GraphicsUnit.Pixel);
            var graphics = Graphics.FromImage(bmp);

            graphics.Clear((Color)new ColorConverter().ConvertFromString("#" + bgColour));
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.DrawString(avatarString, font, new SolidBrush(Color.WhiteSmoke), new RectangleF(0, 0, 192, 192), sf);

            graphics.Flush();

            using (var memStream = new System.IO.MemoryStream())
            {
                bmp.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                var result = this.File(memStream.GetBuffer(), "image/png");
                return result;
            }
        }

        private string toUniversalTime(DateTime dt)
        {
            string DateFormat = "yyyyMMddTHHmmssZ";
            return dt.ToUniversalTime().ToString(DateFormat);
        }

        //[HttpGet]
        //public ActionResult EmailiCal(string email)
        //{
        //    string iCal = _calService.GetCalendar();
        //    byte[] calBytes = System.Text.Encoding.UTF8.GetBytes(iCal);
        //    Stream stream = new MemoryStream(calBytes);

        //    DateTime dtnow = DateTime.Now;
        //    string strnow = toUniversalTime(DateTime.Now);
        //    string dtstart = toUniversalTime(dtnow.AddDays(1));
        //    string dtend = toUniversalTime(dtnow.AddMinutes(30));

        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("BEGIN:VCALENDAR");
        //    sb.AppendLine("VERSION:2.0");
        //    sb.AppendLine("METHOD:REQUEST");
        //    sb.AppendLine("BEGIN:VTIMEZONE");
        //    sb.AppendLine("TZID:Eastern Standard Time");
        //    sb.AppendLine("BEGIN:STANDARD");
        //    sb.AppendLine("DTSTART:" + dtstart);
        //    sb.AppendLine("RRULE:FREQ=YEARLY;BYDAY=1SU;BYHOUR=2;BYMINUTE=0;BYMONTH=11");
        //    sb.AppendLine("TZNAME:Eastern Standard Time");
        //    sb.AppendLine("TZOFFSETFROM:-0400");
        //    sb.AppendLine("TZOFFSETTO:-0500");
        //    sb.AppendLine("END:STANDARD");
        //    sb.AppendLine("BEGIN:DAYLIGHT");
        //    sb.AppendLine("DTSTART:" + dtstart);
        //    sb.AppendLine("RRULE:FREQ=YEARLY;BYDAY=2SU;BYHOUR=2;BYMINUTE=0;BYMONTH=3");
        //    sb.AppendLine("TZNAME:Eastern Daylight Time");
        //    sb.AppendLine("TZOFFSETFROM:-0500");
        //    sb.AppendLine("TZOFFSETTO:-0400");
        //    sb.AppendLine("END:DAYLIGHT");
        //    sb.AppendLine("END:VTIMEZONE");
        //    sb.AppendLine("BEGIN:VEVENT");
        //    sb.AppendLine("ATTENDEE;CN=John Doe;ROLE=REQ-PARTICIPANT;PARTSTAT=TENTATIVE;CUTYPE=INDIVIDUAL;RSVP=TRUE:MAILTO:john.doe@outlook.com");
        //    sb.AppendLine("DESCRIPTION:");
        //    sb.AppendLine("DTEND;TZID=Eastern Standard Time:" + dtend);
        //    sb.AppendLine("DTSTAMP:" + strnow);
        //    sb.AppendLine("DTSTART;TZID=Eastern Standard Time:" + dtstart);
        //    sb.AppendLine("LAST-MODIFIED:" + strnow);
        //    sb.AppendLine("LOCATION:");
        //    sb.AppendLine("ORGANIZER;CN=Face My Resume:MAILTO:noreply@lsinextgen.com");
        //    sb.AppendLine("SEQUENCE:0");
        //    sb.AppendLine("ACTION:DISPLAY");
        //    sb.AppendLine("SUMMARY:Nevermind");
        //    sb.AppendLine("UID:" + Guid.NewGuid().ToString());
        //    sb.AppendLine("END:VEVENT");
        //    sb.AppendLine("END:VCALENDAR");

        //    ContentType contentType = new ContentType("text/calendar");
        //    contentType.Parameters.Add("method", "REQUEST");
        //    contentType.Parameters.Add("component", "VEVENT");
        //    contentType.Parameters.Add("content-disposition", "inline;filename=meeting_" + _helperService.RandomString(6, false) + ".ics");

        //    AlternateView calendarView = AlternateView.CreateAlternateViewFromString(sb.ToString(), contentType);
        //    calendarView.TransferEncoding = TransferEncoding.SevenBit;

        //    Attachment calfile = new Attachment(stream, contentType);
        //    IList<string> cc = new List<string>();

        //    _emailService.SendEmailAsync(email, cc, "Interview Schedule", "You are invited for an Interview", calendarView);
        //    return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Mail sent Successfully" });
        //}

        //[HttpGet]
        //public ActionResult EmailiCal2(int evtid, string email)
        //{
        //    var evtcal = _context.Event
        //                 .Include(x => x.Participants)
        //                 .Where(x => (x.Id == evtid)).SingleOrDefault();

        //    if (evtcal == null)
        //    {
        //        return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Not a valid event." });
        //    }
        //    var attendees = evtcal.Participants;
        //    if (attendees.Count <= 0)
        //    {
        //        return Ok(new { StatusCode = StatusCodes.Status200OK, result = "No attendees added to the event." });
        //    }

        //    string calstr = _calService.GetCalendar(evtcal);
        //    ContentType contentType = new ContentType("text/calendar");
        //    contentType.Parameters.Add("method", "REQUEST");
        //    contentType.Parameters.Add("component", "VEVENT");
        //    contentType.Parameters.Add("content-disposition", "inline;filename=meeting_" + _helperService.RandomString(6, false) + ".ics");

        //    AlternateView calendarView = AlternateView.CreateAlternateViewFromString(calstr, contentType);
        //    calendarView.TransferEncoding = TransferEncoding.SevenBit;
        //    IList<string> cc = new List<string>();

        //    _emailService.SendEmailAsync(email, cc, "FMR Interview Schedule", "You are invited for an Interview", calendarView);
        //    return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Mail sent Successfully" });
        //}

        [HttpGet]
        public async Task<IActionResult> JobDetail(int joborderid)
        {
                 var jobOrders = await _context.JobOrder.Where(x => x.Id == joborderid)
                 .Select(r => new
                 {
                     Id = r.Id,
                     jobtitle = r.Title,
                     jobtypeid = r.JobTypeId,
                     jobtype = r.JobType.Type,
                     skills = r.Skills,
                     openings = r.Openings,
                     experience = r.Experience,
                     company = r.Company,
                    // location = r.Company.Address.City.Name + ", " + r.Company.Address.City.State.Code,
                     location = r.Location,
                     companyname = r.CompanyName,
                     published = r.Published,
                     summary = r.Summary,
                     notes = r.Notes
                   
                 })
                .SingleOrDefaultAsync();
                if (jobOrders == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobOrders });
            }

        [HttpGet]
        public async Task<IActionResult> Assessment(string code)
        {
                var query = await _context.JobOrder
                 .Include(x => x.Assessments).ThenInclude(x => x.Candidate)
                 .Where(x => x.InviteId== code)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    jobtypeid = x.JobType.Id,
                    jobtype = x.JobType.Type,
                    location = x.Company.Address.City.Name + ", " + x.Company.Address.City.State.Code,
                    experience = x.Experience,
                    openings = x.Openings,
                    skills = x.Skills,
                    addedon = x.Created,
                    //published = x.Published,
                    updated = x.Updated,
                    status = x.Status,
                    companyname=x.CompanyName,
                    applied = x.Assessments.Where(a => a.AssessmentStatusId == 1).OrderByDescending(o => o.UpdatedOn)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    screening = x.Assessments.Where(a => a.AssessmentStatusId == 2).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    shortlisted = x.Assessments.Where(a => a.AssessmentStatusId == 3).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    onboarding = x.Assessments.Where(a => a.AssessmentStatusId == 4).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    rejected = x.Assessments.Where(a => a.AssessmentStatusId == 5).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    parked = x.Assessments.Where(a => a.AssessmentStatusId == 6).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                }).SingleOrDefaultAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
          
        }

        [HttpGet]
        public async Task<IActionResult> Assessment1(int JoborderId)
        {
            var query = await _context.JobOrder
             .Include(x => x.Assessments).ThenInclude(x => x.Candidate)
             .Where(x => x.Id == JoborderId)
            .Select(x => new
            {
                id = x.Id,
                title = x.Title,
                jobtypeid = x.JobType.Id,
                jobtype = x.JobType.Type,
                location = x.Company.Address.City.Name + ", " + x.Company.Address.City.State.Code,
                experience = x.Experience,
                openings = x.Openings,
                skills = x.Skills,
                addedon = x.Created,
                    //published = x.Published,
                    updated = x.Updated,
                status = x.Status,
                applied = x.Assessments.Where(a => a.AssessmentStatusId == 1).OrderBy(o => o.UpdatedOn)
                .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                screening = x.Assessments.Where(a => a.AssessmentStatusId == 2).OrderByDescending(o => o.TotalRating)
                .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                shortlisted = x.Assessments.Where(a => a.AssessmentStatusId == 3).OrderByDescending(o => o.TotalRating)
                .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                onboarding = x.Assessments.Where(a => a.AssessmentStatusId == 4).OrderByDescending(o => o.TotalRating)
                .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                rejected = x.Assessments.Where(a => a.AssessmentStatusId == 5).OrderByDescending(o => o.TotalRating)
                .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                parked = x.Assessments.Where(a => a.AssessmentStatusId == 6).OrderByDescending(o => o.TotalRating)
                .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
            }).SingleOrDefaultAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });

        }

        [HttpGet]
        public async Task<IActionResult> Evaluation(int assessmentid)
        {


                var query = await _context.Assessment
                    .Include(x => x.Responses)
                    .Include(x => x.Candidate).ThenInclude(x => x.User)
                    .Include(x => x.JobOrder)
                   .Where(x => x.Id == assessmentid)
                   .Select(x => new
                   {
                       assessmentid = x.Id,
                       rating = x.TotalRating,
                       uploadedon = x.UpdatedOn,
                       Candidate = new { id = x.Candidate.Id, name = x.Candidate.Name, position = x.Candidate.Position, skills = x.Candidate.Skills, userid = x.Candidate.UserId, photo = _helperService.GetUserPhoto(x.Candidate.User) },
                       Responses = _context.vwCandidateJob
                       .Where(a => a.AssessmentId == x.Id)
                       .Select(r => new {
                           id = r.Id,
                           questionid = r.QuestionId,
                           question = r.QuestionTitle,
                           video = string.IsNullOrEmpty(r.Videofile) ? r.Videofile : _helperService.GetMediaUrl(x.Candidate.User.UserGuid.ToString()) + "/" + r.Videofile,
                           rating = r.Rating,
                           notes = r.Notes,
                           duration = r.Duration,
                           orderbyid = r.OrderById,
                           status = r.Status,
                           description = r.Description

                       }).OrderBy(o => o.orderbyid).ToList()
                   })
                 .SingleOrDefaultAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            
        }

        [HttpPost]
        public async Task<IActionResult> Evaluvate([FromBody]Evaluate evaluate)
        {
       
                var Ac = new AssesmentCandidate();
                Ac.Id = evaluate.responseid;
                _context.AssesmentCandidate.Attach(Ac);
                Ac.Rating = evaluate.rating;
                Ac.Notes = evaluate.notes;
                Ac.Status = 3;
                await _context.SaveChangesAsync();
                _context.Entry<AssesmentCandidate>(Ac).State = EntityState.Detached;

                var ac = await _context.AssesmentCandidate
                  .Where(a => a.Id == evaluate.responseid)
                  .SingleOrDefaultAsync();

                var Assments = await _context.vwCandidateJob
                  .Where(a => a.AssessmentId == ac.AssessmentId)
                  .ToListAsync();

                int resptot = Assments.Count;
                //  var rattot = Assments.GroupBy(o => o.AssessmentId).Select(x => new { total = x.Sum(i => i.Rating) });
                int rattotal = 0;

                foreach (var rat in Assments)
                {
                    rattotal += rat.Rating.Value;
                }
                if (rattotal != 0 && resptot != 0)
                {
                    var nasmnt = new Assessment();
                    nasmnt.Id = ac.AssessmentId;
                    _context.Assessment.Attach(nasmnt);
                    nasmnt.TotalRating = rattotal / resptot;
                    nasmnt.AssessmentStatusId = 2;
                    await _context.SaveChangesAsync();
                }
                return await Evaluation(ac.AssessmentId);
           
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody]StatusDTO status)
        {

            Assessment assmt = new Assessment();
            assmt.Id = status.assessmentId;
            _context.Assessment.Attach(assmt);
            assmt.AssessmentStatusId = status.statusId;
            await _context.SaveChangesAsync();
            _context.Entry<Assessment>(assmt).State = EntityState.Detached;
            var assmnt = await _context.Assessment.Where(a => a.Id == status.assessmentId).SingleOrDefaultAsync();
            return await Assessment1(assmnt.JobOrderId);

        }

        [HttpGet]
        public async Task<IActionResult> Interviews(int assessmentid)
        {
           

                var assmt = await _context.Assessment
                    .Include(x => x.Calendar).Where(x => x.Id == assessmentid).SingleOrDefaultAsync();
                _context.Entry<Assessment>(assmt).State = EntityState.Detached;

                if (assmt.CalendarId == null)
                {
                    Calendar cal = new Calendar();
                    cal.AddedOn = DateTime.Now;
                    _context.Calendar.Add(cal);
                    await _context.SaveChangesAsync();

                    Assessment Ast = new Assessment();
                    Ast.Id = assmt.Id;
                    _context.Assessment.Attach(Ast);
                    Ast.CalendarId = cal.Id;
                    await _context.SaveChangesAsync();
                    _context.Entry<Calendar>(cal).State = EntityState.Detached;
                    _context.Entry<Assessment>(Ast).State = EntityState.Detached;

                }

                var query = await _context.Assessment
                    .Include(x => x.JobOrder)
                   .Where(x => x.Id == assessmentid)
                   .Select(x => new
                   {
                       assessmentid = x.Id,
                       rating = x.TotalRating,
                       uploadedon = x.UpdatedOn,
                       Candidate = _context.Candidate.Include(c => c.User).Where(c => c.Id == x.CandidateId).Select(c => new { id = c.Id, name = c.Name, position = c.Position, skills = c.Skills, userid = c.UserId, photo = _helperService.GetUserPhoto(c.User) }).SingleOrDefault(),
                       Calendar = _context.Calendar.Include(ce => ce.Events).Where(ca => ca.Id == x.CalendarId).Select(ca => new
                       {
                           id = ca.Id,
                           invitesent = ca.InviteSent,
                           senton = ca.SentOn,
                           events = x.Calendar.Events.Select(e => new
                           {
                               id = e.Id,
                               title = e.Title,
                               start = e.StartTime,
                               end = e.EndTime,
                               description = e.Description,
                               location = e.Location,
                               status = e.Status,
                               participants = e.Participants.Where(p => p.EventId == e.Id)
                               .Select(p => new
                               {
                                   id = p.ParticipantId,
                                   name = p.Participant.FullName,
                                   email = p.Participant.Email,
                                   status = p.Status,
                                   photo = _helperService.GetUserPhoto(p.Participant),
                                   organizer = p.IsOrganizer
                               }).ToList()
                           })
                       }).SingleOrDefault()
                   })
                 .SingleOrDefaultAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
      
        }

        [HttpGet]
        public async Task<IActionResult> Event(int Id)
        {
           
                var Event = await _context.Event
               .Where(x => ((x.EventType == 2) && (x.Id == Id)))
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
                   participants = x.Participants.Where(e => e.EventId == x.Id)
                   .Select(p => new
                   {
                       id = p.ParticipantId,
                       name = p.Participant.FullName,
                       email = p.Participant.Email,
                       status = p.Status,
                       photo = _helperService.GetUserPhoto(p.Participant),
                       organizer = p.IsOrganizer
                   }).ToList(),
               }).SingleOrDefaultAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = Event });
          
        }

        
   

    }
}
