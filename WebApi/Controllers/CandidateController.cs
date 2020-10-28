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
using System;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.IO;
using MimeKit;
using System.Text;

namespace WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
   [Authorize(Roles = "Candidate")]
    public class CandidateController : ControllerBase
    {

        private readonly DataContext _context;
        private readonly IHelperService _helperService;
        private readonly IEmailService _emailservice;

        public CandidateController(DataContext context, IHelperService helperService, IEmailService emailService)
        {
            _context = context;
            _helperService = helperService;
            _emailservice = emailService;
        }


        [HttpGet]
        public async Task<ActionResult> MyJobs([FromQuery]Paging pg)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.JobCandidate
                    .Include(j => j.JobOrder)
                    .Include(c => c.Candidate).ThenInclude(u => u.User)
                  .Where(c => (c.Candidate.User.Id == user.Id) && (!c.IsDeleted))
                  .Select(x => new
                  {
                      jobrrderid = x.jobOrderId,
                      title = x.JobOrder.Title,
                      experience = x.JobOrder.Experience,
                      skills = x.JobOrder.Skills,
                      openings = x.JobOrder.Openings,
                      summary = x.JobOrder.Summary,
                      notes = x.JobOrder.Notes,
                      status = x.JobOrder.Status,
                      companyid = x.JobOrder.CompanyId,
                      company = x.JobOrder.Company.Name,
                      location = x.JobOrder.Location,
                      companyname = x.JobOrder.CompanyName,
                     // location = x.JobOrder.Company.Address.City.Name + ", " + x.JobOrder.Company.Address.City.State.Code,
                      jobTypeid = x.JobOrder.JobTypeId,
                      jobtype = x.JobOrder.JobType.Type,
                      published = x.JobOrder.Published,
                      updated = x.JobOrder.Updated,
                      isactive = x.JobOrder.IsActive,
                      candidateId = x.CandidateId,
                      assesment = x.JobOrder.Assessments.Where(a=> (a.JobOrderId==x.jobOrderId && a.CandidateId== x.CandidateId)).Select(a => new { id = a.Id, status = a.AssessmentStatusId }).FirstOrDefault()
                  });

                query = pg.q != "" ? query.Where(x => (x.title.Contains(pg.q)) || (x.location.Contains(pg.q)) || (x.skills.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.published) : query = query.OrderBy(w => w.published); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = paged, result = qdata });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyJob([FromBody]applyDTO jcid)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var assessment = await _context.Assessment.Where(x => (x.JobOrderId == jcid.jobid) && (x.CandidateId == jcid.candidateid)).SingleOrDefaultAsync();
                int asmid;
                if (assessment == null)
                {
                    Assessment nasses = new Assessment { JobOrderId = jcid.jobid, CandidateId = jcid.candidateid, UpdatedOn = DateTime.Now, AssessmentStatusId = 0, TotalRating = 0 };
                    Assessment anew = PostAssessment(nasses);
                    asmid = anew.Id;
                }
                else
                {
                    asmid = assessment.Id;
                }
                return  await Questions(asmid);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Questions(int assessmentid)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var CandidateJob = await _context.vwCandidateJob
                 .Where(x => (x.AssessmentId == assessmentid))
                 .Select(x=> new
                 {
                     assessmentid=x.AssessmentId,
                     questionid = x.QuestionId,
                     question =  (x.Status==null || x.Status==0)  ? "Question": x.QuestionTitle,
                     description = (x.Status == null || x.Status == 0) ? "Description" : x.Description,
                     duration = x.Duration,
                     Buffer = ((x.BufferTime == null) ? 0 : x.BufferTime),
                     uploadedon = x.UploadedOn,
                     video = string.IsNullOrEmpty(x.Videofile)? x.Videofile  : _helperService.GetMediaUrl(user.UserGuid.ToString()) + "/" + x.Videofile,
                     orderid= x.OrderById,
                     status = x.Status
                 }).OrderBy(x=>x.orderid)
                 .ToListAsync();

                var docs = await _context.DocumentTemplate.ToListAsync();
                var forms = await _context.FormTemplate.ToListAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = CandidateJob, result2 = docs, result3=forms });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ShowQuestion([FromQuery]AnswerDTO ques)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var acc = await _context.AssesmentCandidate.Where(x => (x.AssessmentId == ques.assessmentid) && (x.QuestionId == ques.questionid)).SingleOrDefaultAsync();
                AssesmentCandidate ac = new AssesmentCandidate();
                if (acc == null)
                {
                    ac.AssessmentId = ques.assessmentid;
                    ac.QuestionId = ques.questionid;
                    ac.Status = 1;
                    _context.AssesmentCandidate.Add(ac);
                }
                else
                {
                    _context.Entry<AssesmentCandidate>(acc).State = EntityState.Detached;
                    ac.Id = acc.Id;
                    _context.AssesmentCandidate.Attach(ac);
                    ac.Status = 1;
                }
                await _context.SaveChangesAsync();
                return await Question(ques.questionid, ques.assessmentid);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Question(int questionid, int assessmentid)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var CandidateJob = await _context.vwCandidateJob
                 .Where(x => (x.QuestionId == questionid && x.AssessmentId == assessmentid))
                 .Select(x => new
                 {
                     assessmentid = x.AssessmentId,
                     questionid = x.QuestionId,
                     question = (x.Status == null || x.Status == 0) ? "Question" : x.QuestionTitle,
                     description = (x.Status == null || x.Status == 0) ? "Description" : x.Description,
                    //duration = x.Duration + x.BufferTime,
                     duration = x.Duration,
                     buffer = x.BufferTime,
                     uploadedon = x.UploadedOn,
                     video = string.IsNullOrEmpty(x.Videofile) ? x.Videofile : _helperService.GetMediaUrl(user.UserGuid.ToString()) + "/" + x.Videofile,
                     orderid = x.OrderById,
                     status = x.Status
                  
                 }).OrderBy(x => x.orderid).SingleOrDefaultAsync();
            
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = CandidateJob });
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Answer(IFormFile file,[FromForm] AnswerDTO answer)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                try
                {
                    if (file.Length > 0)
                    {
                            string fileName = ""; fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                            FileInfo fi = new FileInfo(fileName);
                            string fileN = _helperService.RandomString(8, true) + fi.Extension;
                            string strurl = _helperService.GetMediaUrl(user.UserGuid.ToString()) +"/"+ fileN;
                            string fullPath = Path.Combine(_helperService.GetFilePath(user.UserGuid.ToString()),fileN);
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                        var acc = await _context.AssesmentCandidate.Where(x => (x.AssessmentId == answer.assessmentid) && (x.QuestionId==answer.questionid)).SingleOrDefaultAsync();
                        AssesmentCandidate ac = new AssesmentCandidate();
                        if (acc==null)
                        {
                            ac.Videofile = fileN;
                            ac.UploadedOn = DateTime.Now;
                            ac.AssessmentId = answer.assessmentid;
                            ac.QuestionId = answer.questionid;
                            ac.Status = 2;
                            _context.Add(ac);
                        }
                        else
                        {
                            _context.Entry<AssesmentCandidate>(acc).State = EntityState.Detached;
                            ac.Id = acc.Id;
                            _context.AssesmentCandidate.Attach(ac);
                            ac.Videofile = fileN;
                            ac.AssessmentId = answer.assessmentid;
                            ac.QuestionId = answer.questionid;
                            ac.UploadedOn = DateTime.Now;
                            ac.Status = 2;
                        }
                        await _context.SaveChangesAsync();
                        return await Question(answer.questionid, answer.assessmentid);
                    }
                    else
                    {
                        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Error Submitting data.", result = "" });
                    }
                }
                catch (System.Exception ex)
                {
                    return Ok(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAssessment([FromBody] StatusDTO assessment)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var acc = await _context.Assessment.Where(x => (x.Id == assessment.assessmentId)).SingleOrDefaultAsync();

                if (acc == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Invalid assessment info.", result = "" });
                }
                else
                {
                    _context.Entry<Assessment>(acc).State = EntityState.Detached;
                    Assessment ac = new Assessment();
                    ac.Id = acc.Id;
                    _context.Assessment.Attach(ac);
                    ac.AssessmentStatusId = 1;
                    await _context.SaveChangesAsync();
                  

                    var ajc = _context.Assessment
                        .Include(c => c.Candidate)
                        .Include(j => j.JobOrder).ThenInclude(m => m.Manager).Where(x => (x.Id == assessment.assessmentId))
                         .Select(x => new
                         {
                             manageremail = x.JobOrder.Manager.Email,
                             candidatename = x.Candidate.Name,
                             jobname = x.JobOrder.Title
                         }).SingleOrDefault();

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>Face My Resume</title>");
                    sb.AppendLine("<style type=\"text/css\"> body {font-family: \"Lato\", \"Lucida Grande\", \"Lucida Sans Unicode\", Tahoma, Sans-Serif; font-siz:18px;}</style>");
                    sb.AppendLine("</head><body><div style=\"text-align:center\">");
                    sb.AppendLine("<p>Hello \"" +ajc.candidatename+ "\" </p>");
                    sb.AppendLine("<p>Thanks so much for your interest with our LSINextGen Team Will Review your qualifications and contact to you if your a potential match for \"" + ajc.jobname + "\" position</p>");
                    sb.AppendLine("<p>At LSINextGen ,we are always on the look out for top tier talent for both technincal and non technical roles.</ p >");
                    sb.AppendLine("<p><strong>The FMR Team</strong><br><a href=\"https://www.facemyresume.com\">www.facemyresume.com</a></p></div></body></html>");

                    IList<string> lstEmail = new List<string> { ajc.manageremail};
                   
                  await  _emailservice.SendEmailAsync(lstEmail, null, "A new assessment has been submitted", sb.ToString());
                }
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Assessment submitted successfully", result = "" });

            }
        }

        [HttpGet]
        public async Task<ActionResult> Interviews([FromQuery]Paging pg)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
         var query = _context.EventUser
          .Include(x=>x.Event).ThenInclude(c=>c.Calendar)
          .Where(x=>x.ParticipantId==user.Id && x.Event.Calendar.InviteSent==true)
        .Select(x => new
        {
            id = x.Id,
            starttime = x.Event.StartTime,
            endtime = x.Event.EndTime,
            subject = x.Event.Title,
            description = x.Event.Description,
            updatedon = x.Event.UpdatedOn,
            calendarid = x.Event.CalendarId,
            eventtype = x.Event.EventType,
            location = x.Event.Location,
            eventstatus = x.Event.Status,
            participants = x.Event.Participants
            .Select(p => new
            {
                id= p.Id,
                name = p.Participant.FullName,
                email = p.Participant.Email,
                photo= _helperService.GetUserPhoto(p.Participant),
                status = p.Status,
                organizer = p.IsOrganizer
            }).ToList(),
         });

                query = pg.q != "" ? query.Where(x => (x.subject.Contains(pg.q)) || (x.description.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobOffers([FromQuery]Paging pg)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.Assessment
                    .Include(x => x.OnBoarding)
                    .Include(x => x.Candidate)
                    .Include(x => x.JobOrder)
                    .ThenInclude(x=>x.Company)
                    .ThenInclude(x=>x.Address)
                   .Where(x => (x.Candidate.UserId == user.Id) && x.AssessmentStatusId==4 && x.OnBoarding.StatusId==1)
                   .Select(x => new
                   {
                       assessmentid = x.Id,
                       rating = x.TotalRating,
                       updatedon = x.UpdatedOn,
                       joborder = new
                       {
                           id = x.JobOrder.Id,
                           title = x.JobOrder.Title,
                           jobtypeid = x.JobOrder.JobType.Id,
                           jobtype = x.JobOrder.JobType.Type,
                           company = x.JobOrder.Company.Name,
                           location = x.JobOrder.Company.Address.City.Name + ", " + x.JobOrder.Company.Address.City.State.Code,
                           experience = x.JobOrder.Experience,
                           skills = x.JobOrder.Skills,
                           managerid = x.JobOrder.Manager.Id,
                           manager = x.JobOrder.Manager.FullName,
                           status = x.JobOrder.Status
                       },
                    //   Candidate = new { id = x.Candidate.Id, name = x.Candidate.Name, position = x.Candidate.Position, skills = x.Candidate.Skills, userid = x.Candidate.UserId, photo = x.Candidate.user.Photo },
                       onBoarding = new {
                           id = x.OnBoarding.Id,
                           joindate = x.OnBoarding.JoiningDate,
                           message = x.OnBoarding.Message,
                           filename = _helperService.GetMediaUrl(x.JobOrder.Company.UID.ToString()) + "/" + x.OnBoarding.FilePath,
                           addedbyid = x.OnBoarding.AddedById,
                           addedby = x.OnBoarding.AddedBy.FullName,
                           status = x.OnBoarding.StatusId,
                           addedon = x.OnBoarding.AddedOn }
                   });

                query = pg.q != "" ? query.Where(x => (x.joborder.title.Contains(pg.q)) || (x.joborder.company.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.updatedon) : query = query.OrderBy(w => w.updatedon); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = paged, result = qdata });
            }
        }

        [HttpPost]
        private  Assessment PostAssessment(Assessment Assessment)
        {
            _context.Assessment.Add(Assessment);
            _context.SaveChanges();

            return  Assessment;
        }

        [HttpGet]
        public async Task<IActionResult> PracticeQuestions()
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var CandidateJob = await _context.vwPractice
                 .Where(x => (x.CandidateId == user.Id || x.Id == null))
                 .Select(x => new
                 {
                     questionid = x.QuestionId,
                     question = (x.Status == null || x.Status == 0) ? "Question" : x.QuestionTitle,
                     duration = x.Duration,
                     uploadedon = x.UploadedOn,
                     video = string.IsNullOrEmpty(x.VideoFile) ? x.VideoFile : _helperService.GetMediaUrl(user.UserGuid.ToString()) + "/" + x.VideoFile,
                     status = x.Status,
                     description = x.Description
                 }).OrderBy(x => x.questionid)
                 .ToListAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = CandidateJob });
            }
        }

        [HttpGet]
        public async Task<IActionResult> practicequestion([FromQuery]AnswerDTO ques)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var acc = await _context.PracticeCandidate.Where(x => (x.CandidateId == user.Id) && (x.QuestionId == ques.questionid)).SingleOrDefaultAsync();
                PracticeCandidate ac = new PracticeCandidate();
                if (acc == null)
                {
                    ac.CandidateId = Convert.ToInt32(user.Id);
                    ac.QuestionId = ques.questionid;
                    ac.Status = 1;
                    _context.PracticeCandidate.Add(ac);
                }
                else
                {
                    _context.Entry<PracticeCandidate>(acc).State = EntityState.Detached;
                    ac.Id = acc.Id;
                    _context.PracticeCandidate.Attach(ac);
                    ac.Status = 1;
                }
                await _context.SaveChangesAsync();
                return await PQuestion(ques.questionid);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PQuestion(int questionid)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var CandidateJob = await _context.vwPractice
                 .Where(x => (x.QuestionId == questionid && x.CandidateId == user.Id))
                 .Select(x => new
                 {
                     assessmentid = x.CandidateId,
                     questionid = x.QuestionId,
                     question = (x.Status == null || x.Status == 0) ? "Question" : x.QuestionTitle,
                     // duration = x.Duration + x.BufferTime,
                     duration = x.Duration,
                     uploadedon = x.UploadedOn,
                     video = string.IsNullOrEmpty(x.VideoFile) ? x.VideoFile : _helperService.GetMediaUrl(user.UserGuid.ToString()) + "/" + x.VideoFile,
                     status = x.Status,
                     description = x.Description
                 }).OrderBy(x => x.questionid).SingleOrDefaultAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = CandidateJob });
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> practiceanswer(IFormFile file, [FromForm] AnswerDTO answer)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                try
                {
                    if (file.Length > 0)
                    {
                        string fileName = ""; fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        FileInfo fi = new FileInfo(fileName);
                        string fileN = _helperService.RandomString(8, true) + fi.Extension;
                        string strurl = _helperService.GetMediaUrl(user.UserGuid.ToString()) + "/" + fileN;
                        string fullPath = Path.Combine(_helperService.GetFilePath(user.UserGuid.ToString()), fileN);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        var acc = await _context.PracticeCandidate.Where(x => (x.CandidateId == user.Id) && (x.QuestionId == answer.questionid)).SingleOrDefaultAsync();
                        PracticeCandidate ac = new PracticeCandidate();
                        if (acc == null)
                        {
                            ac.VideoFile = fileN;
                            ac.UploadedOn = DateTime.Now;
                            ac.CandidateId = Convert.ToInt32(user.Id);
                            ac.QuestionId = answer.questionid;
                            ac.Status = 2;
                            _context.Add(ac);
                        }
                        else
                        {
                            _context.Entry<PracticeCandidate>(acc).State = EntityState.Detached;
                            ac.Id = acc.Id;
                            _context.PracticeCandidate.Attach(ac);
                            ac.VideoFile = fileN;
                            ac.CandidateId = Convert.ToInt32(user.Id);
                            ac.QuestionId = answer.questionid;
                            ac.UploadedOn = DateTime.Now;
                            ac.Status = 2;
                        }
                        await _context.SaveChangesAsync();
                        return await PQuestion(answer.questionid);
                    }
                    else
                    {
                        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Error Submitting data.", result = "" });
                    }
                }
                catch (System.Exception ex)
                {
                    return Ok(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> PReset()
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                var ques = await _context.PracticeCandidate.Where(p => p.CandidateId == user.Id)
    .ToListAsync();

                if (ques == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                _context.PracticeCandidate.RemoveRange(ques);

                await _context.SaveChangesAsync();

                return await PracticeQuestions();
            }
        }

        [HttpPost]
        public async Task<IActionResult> submitpractice([FromBody] StatusDTO assessment)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var acc = await _context.Assessment.Where(x => (x.Id == assessment.assessmentId)).SingleOrDefaultAsync();

                if (acc == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Invalid assessment info.", result = "" });
                }
                else
                {
                    _context.Entry<Assessment>(acc).State = EntityState.Detached;
                    Assessment ac = new Assessment();
                    ac.Id = acc.Id;
                    _context.Assessment.Attach(ac);
                    ac.AssessmentStatusId = 1;
                    await _context.SaveChangesAsync();
                }
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Assessment submitted successfully", result = "" });
            }
        }



    }
}