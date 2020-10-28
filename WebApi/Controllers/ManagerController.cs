using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;
using WebApi.Services;
using WebApi.Models;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.IO;
using TinyCsvParser;
using System.Text;
using EFCore.BulkExtensions;

namespace WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin, Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHelperService _helperService;
        private readonly IEmailService _emailService;

        public ManagerController(DataContext context, IHelperService helperService, IEmailService EmailService)
        {
            _context = context;
            _helperService = helperService;
            _emailService = EmailService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var qdata = "";
            return Ok(new { StatusCode = StatusCodes.Status200OK,result = qdata });
        }

        [HttpGet]
        public async Task<IActionResult> Joblist([FromQuery]Paging pg)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.JobOrder
                 .Where(x => (x.CompanyId == user.CompanyId) && (x.ManagerId == user.Id))
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    jobtypeid = x.JobType.Id,
                    jobtype = x.JobType.Type,
                    company = x.Company,
                   // location = x.Company.Address.City.Name + ", " + x.Company.Address.City.State.Code,
                    location = x.Location,
                    companyname = x.CompanyName,
                    experience = x.Experience,
                    openings = x.Openings,
                    skills = x.Skills,
                    addedon = x.Created,
                    published = x.Published,
                    updated = x.Updated,
                    status = x.Status,
                    userid = x.UserId,
                    managerid = x.ManagerId
            }) ;

                query = pg.q != "" ? query.Where(x => (x.title.Contains(pg.q)) || (x.location.Contains(pg.q)) || (x.skills.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JLinfo(int joborderid)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jb = await _context.JobOrder
                 .Where(x => (x.Id == joborderid))
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    jobtypeid = x.JobType.Id,
                    jobtype = x.JobType.Type,
                    company = x.Company,
                    // location = x.Company.Address.City.Name + ", " + x.Company.Address.City.State.Code,
                    location = x.Location,
                    companyname = x.CompanyName,
                    experience = x.Experience,
                    openings = x.Openings,
                    skills = x.Skills,
                    addedon = x.Created,
                    published = x.Published,
                    updated = x.Updated,
                    status = x.Status,
                    userid = x.UserId,
                    managerid = x.ManagerId
                }).SingleOrDefaultAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jb });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JoblistDetail (int joborderid)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jobOrders = await _context.JobOrder.Where(x => x.Id == joborderid)
                 .Select(r => new
                 {
                     Id= r.Id,
                     jobtitle = r.Title,
                     jobtypeid = r.JobTypeId,
                     jobtype = r.JobType.Type,
                     skills = r.Skills,
                     openings = r.Openings,
                     experience = r.Experience,
                     company = r.Company,
                     //   location = r.Company.Address.City.Name + ", " + r.Company.Address.City.State.Code,
                     location = r.Location,
                     companyname = r.CompanyName,
                     published = r.Published,
                     summary = r.Summary,
                     notes = r.Notes,
                     MCQuestions=_context.JobMCQuestion.Where(x=>x.JobOrderId==r.Id).Select(x=>new {id=x.Question.Id,question=x.Question.Question,order=x.OrderById }).OrderBy(y=>y.order).ToList(),
                     Docs= _context.JobOrderDocuments.Where(y=>y.JobOrderId==r.Id).Select(x => new{id = x.Id,documentname = x.Document.DocumentName}).ToList(),
                     invitelink = !string.IsNullOrEmpty(r.InviteId) ? _helperService.GetSiteUrl() + "invite?token=" + r.InviteId : "",
                     Candidates = _context.JobCandidate.Where(j => j.jobOrderId==r.Id).Select(c => new { id = c.Candidate.Id, name = c.Candidate.Name, position = c.Candidate.Position, skills = c.Candidate.Skills, userid = c.Candidate.UserId, photo = _helperService.GetUserPhoto(c.Candidate.User) }).OrderBy(w => w.id).ToList(),
                     Questions = _context.JobQuestion.Where(j => j.JobOrderId == r.Id).Select(q => new { id = q.Question.Id, question = q.Question.QuestionTitle, duration = q.Question.Duration, description = q.Question.Description, qtype= q.Question.QuestionType.TypeName, order = q.OrderById }).OrderBy(w => w.order).ToList()
                 })
                .SingleOrDefaultAsync();
                if (jobOrders == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobOrders });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveJoblist([FromBody]Job job)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                JobOrder jb = new JobOrder();

                jb.Id = job.Id;
                _context.JobOrder.Attach(jb);
                jb.Title = job.Title;
                jb.Experience = job.Experience;
                jb.Skills = job.Skills;
                jb.Openings = job.Openings;
                jb.Summary = job.Summary;
                jb.Location = job.Location;
                jb.CompanyName = job.CompanyName;
                jb.Updated = DateTime.Now;
                jb.JobTypeId = job.JobTypeId;
                jb.ManagerId = job.ManagerId;
                jb.Status = job.Status;
                if (string.IsNullOrEmpty(job.Notes))
                    jb.Notes = "";
                else
                    jb.Notes = job.Notes;


                await _context.SaveChangesAsync();

                if (job.RequiredDocs != null && job.RequiredDocs.Count > 0)
                {
                    var saved = _context.JobOrderDocuments.Where(x => x.JobOrderId == job.Id).ToList();
                    if (saved != null && saved.Count > 0)
                    {
                        _context.RemoveRange(saved);
                    }

                    foreach (var docid in job.RequiredDocs)
                    {
                        JobOrderDocuments documents = new JobOrderDocuments();
                        documents.DocumentId = docid;
                        documents.JobOrderId = job.Id;
                        _context.JobOrderDocuments.Add(documents);
                    }
                   await _context.SaveChangesAsync();
                }
                //  return CreatedAtAction("Joborder", new { id = jb.Id });
                return await JoblistDetail(jb.Id);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveJobOrder([FromBody] Job job)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                JobOrder jb = new JobOrder();
                if (job.Id == 0)
                {
                    jb.Title = job.Title;
                    jb.Experience = job.Experience;
                    jb.Skills = job.Skills;
                    jb.Openings = job.Openings;
                    jb.Summary = job.Summary;
                    jb.Location = job.Location;
                    jb.CompanyName = job.CompanyName;
                    jb.Status = 1;
                    jb.Created = DateTime.Now;
                    jb.Updated = DateTime.Now;
                    jb.IsActive = job.IsActive;
                    jb.JobTypeId = job.JobTypeId;
                    jb.UserId = user.Id;
                    jb.ManagerId = user.Id;
                    jb.CompanyId = user.CompanyId;

                    _context.JobOrder.Add(jb);
                }
                else
                {
                    jb.Id = job.Id;
                    _context.JobOrder.Attach(jb);
                    jb.Title = job.Title;
                    jb.Experience = job.Experience;
                    jb.Skills = job.Skills;
                    jb.Location = job.Location;
                    jb.CompanyName = job.CompanyName;
                    jb.Openings = job.Openings;
                    jb.Summary = job.Summary;
                    jb.Updated = DateTime.Now;
                    jb.JobTypeId = job.JobTypeId;
                    jb.ManagerId = job.ManagerId;
                    jb.Status = job.Status;
                    jb.Notes = job.Notes;
                }
                await _context.SaveChangesAsync();

                //  return CreatedAtAction("Joborder", new { id = jb.Id });
                return await JLinfo(jb.Id);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobOrder(int id)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jobOrder = await _context.JobOrder.FindAsync(id);
                if (jobOrder == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }

                _context.JobOrder.Remove(jobOrder);
                await _context.SaveChangesAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Deleted Successfully", joborder = jobOrder });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobCandidates([FromQuery]Paging pg)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                //                var joborder = _context.JobOrder.Where(x => x.Id == joborderid).SingleOrDefault();
                //               var skillset = joborder.Skills.Split(',');

                var query = _context.Candidate
                // .Where(c => skillset.Any(j=>c.Skills.Contains(j)));
                .Where(x => (x.CompanyId == user.CompanyId))
                .Select(c => new { id = c.Id, name = c.Name, email = c.User.Email, location = c.Address.City.Name + ", " + c.Address.City.State.Code, position = c.Position, skills = c.Skills, userid = c.UserId, photo = _helperService.GetUserPhoto(c.User) });

                query = pg.q != "" ? query.Where(x => (x.name.Contains(pg.q)) || (x.position.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.name) : query = query.OrderBy(w => w.position); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobCandidate(int joborderid)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jobc = await _context.JobCandidate
                 .Include(x => x.Candidate)
                .Where(x => (x.Candidate.CompanyId == user.CompanyId) && (x.jobOrderId == joborderid))
                .Select(c => new
                {
                    id = c.Candidate.Id,
                    name = c.Candidate.Name,
                    email = c.Candidate.User.Email,
                    location = c.Candidate.Address.City.Name + ", " + c.Candidate.Address.City.State.Code,
                    position = c.Candidate.Position,
                    skills = c.Candidate.Skills,
                    userid = c.Candidate.UserId,
                    photo = _helperService.GetUserPhoto(c.Candidate.User)
                }).ToListAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobc });
            }
        }

        [HttpPost]
        public async Task<IActionResult> JobCandidates(int joborderid, jobcandidates candidates)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jcand = await _context.JobCandidate.Where(p => p.jobOrderId == joborderid)
                    .ToListAsync();

                if (jcand != null)
                {
                    _context.JobCandidate.RemoveRange(jcand);
                }
                var Jc = await _context.JobCandidate
               .Where(x => x.jobOrderId == joborderid).ToListAsync();

                List<JobCandidate> jclist = new List<JobCandidate>();
                foreach (Cand jc in candidates.Candidates.Distinct().ToArray())
                {
                    JobCandidate jcan = new JobCandidate();
                    jcan.CandidateId = jc.CandidateId;
                    jcan.AddedById = user.Id;
                    jcan.AddedOn = DateTime.Now;
                    jcan.jobOrderId = joborderid;
                    jclist.Add(jcan);
                }
             await   _context.JobCandidate.AddRangeAsync(jclist);
            //    _context.TryUpdateManyToMany(Jc, jclist.Select(x => new JobCandidate
            //    {
            //        CandidateId = x.CandidateId,
            //        AddedById = x.AddedById,
            //        AddedOn = x.AddedOn,
            //        jobOrderId = joborderid,
            //}), x => x.Id, true);

                await _context.SaveChangesAsync();

                //  return CreatedAtAction("Interviews", new { id = evt.Id });
                return await JobCandidate(joborderid);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveAllCandidates(int joborderid)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                var jcand = await _context.JobCandidate.Where(p => p.jobOrderId == joborderid)
                    .ToListAsync();

                if (jcand == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                _context.JobCandidate.RemoveRange(jcand);

                await _context.SaveChangesAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobQuestions([FromQuery]Paging pg, int? qtypeid)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.Question
                .Where(x => (x.QuestionType.CompanyId == user.CompanyId))
                .Select(q => new
                {
                    id = q.Id,
                    question = q.QuestionTitle,
                    duration = q.Duration,
                    description = q.Description,
                    qtypeid = q.QuestionTypeId,
                    qtype = q.QuestionType.TypeName
                });

                query = qtypeid != null ? query.Where(x => (x.qtypeid == qtypeid)) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.qtypeid) : query = query.OrderBy(w => w.question);

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobQuestion(int joborderid)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jobq = await _context.JobQuestion
                 .Include(x => x.Question)
                .Where(x => (x.Question.QuestionType.CompanyId == user.CompanyId) && x.JobOrderId== joborderid)
                .Select(q => new
                {
                    id = q.Question.Id,
                    question = q.Question.QuestionTitle,
                    duration = q.Question.Duration,
                    description = q.Question.Description,
                    qtypeid = q.Question.QuestionTypeId,
                    qtype = q.Question.QuestionType.TypeName,
                    orderby = q.OrderById
                }).ToListAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobq });
            }
        }

        [HttpPost]
        public async Task<IActionResult> JobQuestions(int joborderid, jobquestions questions)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var ques = await _context.JobQuestion.Where(p => p.JobOrderId == joborderid)
                       .ToListAsync();

                if (ques != null)
                {
                    _context.JobQuestion.RemoveRange(ques);
                }
                
                var Jq = await _context.JobQuestion
               .Where(x => x.JobOrderId == joborderid).ToListAsync();

                List<JobQuestion> jqlist = new List<JobQuestion>();
                foreach (Ques jq in  questions.Questions.Distinct().ToArray())
                {
                    JobQuestion jqtion = new JobQuestion();
                    jqtion.QuestionId = jq.QuestionId;
                    jqtion.OrderById = jq.OrderById;
                    jqtion.AddedById = user.Id;
                    jqtion.AddedOn = DateTime.Now;
                    jqtion.JobOrderId = joborderid;
                    jqlist.Add(jqtion);
                }
                //_context.TryUpdateManyToMany(Jq, jqlist.Select(x => new JobQuestion
                //{
                //    QuestionId = x.QuestionId,
                //    OrderById = x.OrderById,
                //    AddedById = x.AddedById,
                //    AddedOn = x.AddedOn,
                //    JobOrderId = joborderid
                //}), x => x.Id, true);
                await _context.JobQuestion.AddRangeAsync(jqlist);
                await _context.SaveChangesAsync();

                //  return CreatedAtAction("Interviews", new { id = evt.Id });
                return await JobQuestion(joborderid);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveAllQuestions(int joborderid)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
               
                var ques = await _context.JobQuestion.Where(p => p.JobOrderId ==joborderid)
                    .ToListAsync();

                if (ques == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                _context.JobQuestion.RemoveRange(ques);

                await _context.SaveChangesAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PublishJob([FromBody]  publishjob publish)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                var query =  await _context.JobCandidate.Include(x => x.Candidate)
                    .ThenInclude(x=>x.Company)
                    .Where(x => x.jobOrderId == publish.joborderid)
                    .ToListAsync();

                if (query == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records found." });
                }
                else
                {

                    foreach (Candidate cand in query.Select(c => c.Candidate))
                    {
                        try
                        {

                            string token = Guid.NewGuid().ToString("N");

                            InviteCandidate Inv = new InviteCandidate();

                            Inv.Token = token;
                            Inv.CandidateId = cand.Id;
                            Inv.JoborderId = publish.joborderid;
                            Inv.SentOn = DateTime.Now;

                            await _context.InviteCandidate.AddAsync(Inv);
                            await _context.SaveChangesAsync();

                            IList<string> lstEmail = new List<string> { cand.Email };
                            string lnkTxt = _helperService.GetSiteUrl() + "invite?token=" + token;
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>Face My Resume</title>");
                            sb.AppendLine("<style type=\"text/css\"> body {font-family: \"Lato\", \"Lucida Grande\", \"Lucida Sans Unicode\", Tahoma, Sans-Serif; font-siz:18px;}</style>");
                            sb.AppendLine("</head><body><div style=\"text-align:center\"><p><strong>Hi " + cand.Name + ",</strong></p>");
                            sb.AppendLine("<p>A new job matching your profile has been posted by " + cand.Company.Name + " using Face My Resume App, Please use the button below to submit your resume.</p>");
                            sb.AppendLine("<p><a href=\"" + lnkTxt + "\" style=\"background:#d01013; padding:10px 20px; color:#fff; text-decoration:none; font-size:18px; font-weight: 600\">Apply Now</a></p>");
                            sb.AppendLine("<p>Paste the link in your browser if the button is not working.</p><p>" + lnkTxt + "</p>");
                            sb.AppendLine("<p>If your are not interested ignore this email</p>");
                            sb.AppendLine("<p><strong>The FMR Team</strong><br><a href=\"https://www.facemyresume.com\">www.facemyresume.com</a></p></div></body></html>");

                            await _emailService.SendEmailAsync(lstEmail, null, "Job Invitation - Face My Resume", sb.ToString());

                    }
                    catch
                    {

                    }
                }

                    JobOrder jb = new JobOrder();
                    jb.Id = publish.joborderid;
                    _context.JobOrder.Attach(jb);
                    jb.Published = DateTime.Now;
                    jb.InviteId = _helperService.RandomString(6, true) + "-" + _helperService.RandomString(6, true);
                    jb.Status = 2;

                    await _context.SaveChangesAsync();
                    //  return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Job order published successfully."});
                    return await JoblistDetail(publish.joborderid);
                }               
           }

        }

        [HttpGet]
        public async Task<IActionResult> AssesmentJobs([FromQuery]Paging pg)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.JobOrder
                .Include(x => x.Assessments)
                .Where(x => (x.CompanyId == user.CompanyId) && (x.ManagerId == user.Id) && (x.Status == 2))
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    jobtypeid = x.JobType.Id,
                    jobtype = x.JobType.Type,
                   // location = x.Company.Address.City.Name + ", " + x.Company.Address.City.State.Code,
                   location = x.Location,
                    experience = x.Experience,
                    openings = x.Openings,
                    skills = x.Skills,
                    addedon = x.Created,
                    published = x.Published,
                    updated = x.Updated,
                    status = x.Status,
                    applied = x.Assessments.Where(a => a.AssessmentStatusId == 1).Count(),
                    screening = x.Assessments.Where(a => a.AssessmentStatusId == 2).Count(),
                    shortlisted = x.Assessments.Where(a => a.AssessmentStatusId == 3).Count(),
                    onboarding = x.Assessments.Where(a => a.AssessmentStatusId == 4).Count()

                });

                query = pg.q != "" ? query.Where(x => (x.title.Contains(pg.q)) || (x.location.Contains(pg.q)) || (x.skills.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Assessment(int joborderid)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.JobOrder
                 .Include(x => x.Assessments).ThenInclude(x => x.Candidate)
                 .Where(x => (x.CompanyId == user.CompanyId) && (x.ManagerId == user.Id) && (x.Status == 2) && (x.Id == joborderid))
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
                    applied = x.Assessments.Where(a => a.AssessmentStatusId == 1).OrderByDescending(o => o.UpdatedOn)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    screening = x.Assessments.Where(a => a.AssessmentStatusId == 2 && a.Batch==0).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    shortlisted = x.Assessments.Where(a => a.AssessmentStatusId == 3 && a.Batch == 0).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    onboarding = x.Assessments.Where(a => a.AssessmentStatusId == 4 && a.Batch == 0).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    rejected = x.Assessments.Where(a => a.AssessmentStatusId == 5 && a.Batch == 0).OrderByDescending(o => o.TotalRating)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    parked = x.Assessments.Where(a => a.AssessmentStatusId == 6 && a.Batch == 0).OrderByDescending(o => o.TotalRating)
                    .Select(a => new {assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),                    
                    list1 = x.Assessments.Where(a => a.Batch == 1).OrderByDescending(o => o.OrderById)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    list2 = x.Assessments.Where(a => a.Batch == 2).OrderByDescending(o => o.OrderById)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    list3 = x.Assessments.Where(a => a.Batch == 3).OrderByDescending(o => o.OrderById)
                    .Select(a => new { assessmentid = a.Id, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),

                }).SingleOrDefaultAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Assessment2(int joborderid)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.JobOrder
                 .Include(x => x.Assessments).ThenInclude(x => x.Candidate)
                 .Where(x => (x.CompanyId == user.CompanyId) && (x.ManagerId == user.Id) && (x.Status == 2) && (x.Id == joborderid))
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
                    updated = x.Updated,
                    status = x.Status,
                    batch1 = x.Batch1,
                    batch2 = x.Batch2,
                    batch3 = x.Batch3,
                    assessments = x.Assessments.Where(a => (a.Batch == null || a.Batch==0)).OrderByDescending(o => o.UpdatedOn)
                    .Select(a => new { assessmentid = a.Id, status=a.AssessmentStatusId, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    sellists = x.Assessments.Where(a => (a.Batch != null || a.Batch != 0)).OrderBy(o => o.OrderById)
                    .Select(a => new { assessmentid = a.Id, batch=a.Batch, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                }).SingleOrDefaultAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Evaluation(int assessmentid)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.Assessment
                    .Include(x => x.Responses)
                    .Include(x=>x.Documents)
                    .Include(x => x.Forms)
                    .Include(x => x.Candidate).ThenInclude(x=>x.User)
                    .Include(x => x.JobOrder)
                   .Where(x => (x.JobOrder.CompanyId == user.CompanyId) && (x.Id == assessmentid))
                   .Select(x => new
                   {
                       assessmentid = x.Id,
                       rating = x.TotalRating,
                       uploadedon = x.UpdatedOn,
                       Candidate = new { id = x.Candidate.Id, name = x.Candidate.Name, position = x.Candidate.Position, skills = x.Candidate.Skills, userid = x.Candidate.UserId, photo = _helperService.GetUserPhoto(x.Candidate.User)},
                       Responses =  _context.vwCandidateJob
                       .Where(a=>a.AssessmentId==x.Id)
                       .Select(r => new {
                            id = r.Id,
                           questionid =r.QuestionId,
                           question = r.QuestionTitle,
                           video = string.IsNullOrEmpty(r.Videofile) ? r.Videofile : _helperService.GetMediaUrl(x.Candidate.User.UserGuid.ToString()) + "/" + r.Videofile,
                           rating =r.Rating,
                           notes = r.Notes,
                           duration = r.Duration,
                           orderbyid = r.OrderById,
                           status = r.Status,
                           description = r.Description

                       }).OrderBy(o => o.orderbyid).ToList(),
                       Documents = x.Documents,
                       Forms = x.Forms
                   })
                 .SingleOrDefaultAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Evaluvate([FromBody]Evaluate evaluate)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
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
                    nasmnt.AssessmentStatusId =2;
                    await _context.SaveChangesAsync();
                }
                return await Evaluation(ac.AssessmentId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody]StatusDTO status)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                Assessment assmt = new Assessment();
                assmt.Id = status.assessmentId;
                _context.Assessment.Attach(assmt);
                assmt.AssessmentStatusId = status.statusId;
                await _context.SaveChangesAsync();
                _context.Entry<Assessment>(assmt).State = EntityState.Detached;
                var assmnt = await _context.Assessment.Where(a => a.Id == status.assessmentId).SingleOrDefaultAsync();
                return await Assessment2(assmnt.JobOrderId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> updatebatch(int joborderid, int batchid, jobbatches batches)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                foreach (Batch ast in batches.Assessments)
                {
                    Assessment asmt = new Assessment();
                    asmt.Id = ast.AssessmentId;
                    _context.Assessment.Attach(asmt);
                    asmt.Batch = batchid;
                    asmt.OrderById = ast.OrderById;
                    await _context.SaveChangesAsync();
                    _context.Entry<Assessment>(asmt).State = EntityState.Detached;

                }
                return await Assessment2(joborderid);

            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitBatch(int joborderid, int batchid)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                    JobOrder jo = new JobOrder();
                    jo.Id = joborderid;
                    _context.JobOrder.Attach(jo);
                    if(batchid==1)
                        jo.Batch1 = DateTime.Now;
                    else if (batchid == 2)
                        jo.Batch2 = DateTime.Now;
                    if (batchid == 3)
                        jo.Batch3 = DateTime.Now;
                await _context.SaveChangesAsync();
                    _context.Entry<JobOrder>(jo).State = EntityState.Detached;

                JobOrder job1 = await _context.JobOrder.Include(x=>x.Manager).Include(x => x.User).Where(x => x.Id == jo.Id).FirstOrDefaultAsync();
                IList<string> lstEmail = new List<string> { job1.User.Email };
                IList<string>lstCC = new List<string> { user.Email };
                string lnkTxt = _helperService.GetSiteUrl() + "company/assessments?code=" + job1.InviteId;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>Face My Resume</title>");
                sb.AppendLine("<style type=\"text/css\"> body {font-family: \"Lato\", \"Lucida Grande\", \"Lucida Sans Unicode\", Tahoma, Sans-Serif; font-size:18px;}</style>");
                sb.AppendLine("</head><body><div style=\"text-align:center\"><p><strong>Hi " + user.FullName + ",</strong></p>");
                sb.AppendLine("<p>Recruiter "+ job1.Manager.FullName + " has submitted resumes for the " + job1.Title+"</p>");
                sb.AppendLine("<p>Please click the below link to access them,</p>");
                sb.AppendLine("<p><a href=\"" + lnkTxt + "\" style=\"background:#d01013; padding:10px 20px; color:#fff; text-decoration:none; font-size:18px; font-weight: 600\">"+job1.Title+" - Batch "+batchid+"</a></p>");
                sb.AppendLine("<p>Paste the link in your browser if the button is not working.</p><p>" + lnkTxt + "</p>");
                sb.AppendLine("<p><strong>"+user.FullName+"</strong><br><a href=\"https://www.facemyresume.com\">www.facemyresume.com</a></p></div></body></html>");

                await _emailService.SendEmailAsync(lstEmail, lstCC,"FMR : Resumes for "+ job1.Title + " submitted by "+job1.Manager.FullName+", "+job1.CompanyName + " selected candidates - Batch "+ batchid , sb.ToString());
                return await Assessment2(joborderid);

            }
        }


        [HttpPost]
        public async Task<IActionResult> AddBatch([FromBody]BatchDTO batch)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                Assessment assmt = new Assessment();
                assmt.Id = batch.assessmentId;
                _context.Assessment.Attach(assmt);
                assmt.Batch = batch.batchid;
                await _context.SaveChangesAsync();
                _context.Entry<Assessment>(assmt).State = EntityState.Detached;
             
                var assmnt = await _context.Assessment.Where(a => a.Id == batch.assessmentId).SingleOrDefaultAsync();
                return await Assessment2(assmnt.JobOrderId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemBatch([FromBody]int assessmentId)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                Assessment assmt = new Assessment();
                assmt.Id = assessmentId;
                _context.Assessment.Attach(assmt);
                assmt.Batch = null;
                assmt.OrderById = null;
                await _context.SaveChangesAsync();
                _context.Entry<Assessment>(assmt).State = EntityState.Detached;

                var assmnt = await _context.Assessment.Where(a => a.Id == assessmentId).SingleOrDefaultAsync();
                return await Assessment2(assmnt.JobOrderId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> OrderBatch(int joborderid, BatchAssessments batches)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jassments = await _context.Assessment.Where(t => t.JobOrderId == joborderid).ToListAsync();
                foreach (var item in jassments)
                {
                    BatchList bsel = batches.Batches.Find(x => x.assessmentId == item.Id );
                    item.Batch = bsel.batch;
                    item.OrderById = bsel.orderId;
                }
                await _context.SaveChangesAsync();
                return await Assessment2(joborderid);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Interviews(int assessmentid)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                var assmt = await _context.Assessment
                    .Include(x => x.Calendar).Where(x=>x.Id == assessmentid).SingleOrDefaultAsync();
                _context.Entry<Assessment>(assmt).State = EntityState.Detached;

                if (assmt.CalendarId==null)
                {
                    Calendar cal= new Calendar();
                    cal.AddedOn = DateTime.Now;
                    _context.Calendar.Add(cal);
                    await _context.SaveChangesAsync();

                    Assessment Ast = new Assessment();
                    Ast.Id = assmt.Id;
                    _context.Assessment.Attach(Ast);
                    Ast.CalendarId = cal.Id;
                    Ast.AssessmentStatusId = 11;
                    await _context.SaveChangesAsync();
                    _context.Entry<Calendar>(cal).State = EntityState.Detached;
                    _context.Entry<Assessment>(Ast).State = EntityState.Detached;
                   
                }
            
                var query = await _context.Assessment
                    .Include(x => x.JobOrder)
                   .Where(x => (x.JobOrder.CompanyId == user.CompanyId) && (x.Id == assessmentid))
                   .Select(x => new
                   {
                       assessmentid = x.Id,
                       rating = x.TotalRating,
                       uploadedon = x.UpdatedOn,
                       Candidate = _context.Candidate.Include(c=>c.User).Where(c=>c.Id==x.CandidateId).Select(c=> new { id = c.Id, name = c.Name, position = c.Position, skills = c.Skills, userid = c.UserId, photo = _helperService.GetUserPhoto(c.User) }).SingleOrDefault(),
                       Calendar = _context.Calendar.Include(ce=>ce.Events).Where(ca=>ca.Id==x.CalendarId).Select(ca=> new
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
                                   id= p.ParticipantId,
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
        }

        [HttpGet]
        public async Task<IActionResult> Event(int Id)
        {
            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var Event = await _context.Event
               .Where(x => (x.Companyid == comp.Id) && (x.EventType == 2) && (x.Id == Id))
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

        [HttpPost]
        public async Task<IActionResult> AddEvent(int assessmentid,evnt edata)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var asmodel = await _context.Assessment.Include(x => x.Calendar).Include(x=>x.Candidate).Where(x => x.Id == assessmentid).SingleOrDefaultAsync();
              
                Event evt = new Event();
                if (edata.Id == 0)
                {
                    //Calendar caln = new Calendar();
                    //if (asmodel.CalendarId == null)
                    //{
                    //    caln.AddedOn = DateTime.Now;
                    //    _context.Calendar.Add(caln);
                    //    asmodel.CalendarId = caln.Id;
                    //}
                    evt.Title = edata.Title;
                    evt.StartTime = edata.Start;
                    evt.EndTime = edata.End;
                    evt.Description = edata.Description;
                    evt.Status = 0;
                    evt.EventType = 2;
                    evt.Companyid = user.CompanyId;
                    evt.UID = Guid.NewGuid().ToString();
                    evt.CalendarId = asmodel.CalendarId;
                    _context.Add(evt);
                    //Add Candidate
                    //Add Candidate
                    EventUser cuser = new EventUser();
                    cuser.EventId = evt.Id;
                    cuser.ParticipantId = asmodel.Candidate.UserId.GetValueOrDefault();
                    cuser.Status = 0;
                    _context.Add(cuser);

                    //Add Manager
                    EventUser muser = new EventUser();
                    muser.EventId = evt.Id;
                    muser.ParticipantId = user.Id;
                    muser.Status = 0;
                    muser.IsOrganizer = true;
                    _context.Add(muser);

                    List<EventUser> plist = new List<EventUser>();
                    foreach (long pid in edata.Participantids.Distinct().ToArray())
                    {
                        if (pid != user.Id)
                        {
                            EventUser puser = new EventUser();
                            puser.EventId = evt.Id;
                            puser.ParticipantId = pid;
                            puser.Status = 0;
                            _context.Add(puser);
                        }
                    }
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

                    var delpart = await _context.EventUser.Where(p => p.EventId == evt.Id)
                   .ToListAsync();

                    if (delpart != null)
                    {
                        _context.EventUser.RemoveRange(delpart);
                    }
                    //Add Candidate
                    EventUser cuser = new EventUser();
                    cuser.EventId = evt.Id;
                    cuser.ParticipantId = asmodel.Candidate.UserId.GetValueOrDefault();
                    cuser.Status = 0;
                    _context.Add(cuser);

                    //Add Manager
                    EventUser muser = new EventUser();
                    muser.EventId = evt.Id;
                    muser.ParticipantId = user.Id;
                    muser.Status = 0;
                    muser.IsOrganizer = true;
                    _context.Add(muser);

                    List<EventUser> plist = new List<EventUser>();
                    foreach (long pid in edata.Participantids.Distinct().ToArray())
                    {
                        if (pid != user.Id)
                        {
                            EventUser puser = new EventUser();
                            puser.EventId = evt.Id;
                            puser.ParticipantId = pid;
                            puser.Status = 0;
                            _context.Add(puser);
                        }                      
                    }
                   // var model = _context.Event
                   //.Include(x => x.Participants)
                   //.FirstOrDefault(x => x.Id == evt.Id);

                    //_context.TryUpdateManyToMany(model.Participants, plist
                    //                .Select(x => new EventUser
                    //                {
                    //                    ParticipantId = x.ParticipantId,
                    //                    EventId = evt.Id,
                    //                    IsOrganizer = x.IsOrganizer,
                    //                    Status = x.Status
                    //                }), x => x.ParticipantId, true);

                }
                await _context.SaveChangesAsync();

                return await Interviews(asmodel.Id);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendInvite([FromBody]sendinvite inv)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.Assessment
                  .Include(x => x.Calendar).ThenInclude(x => x.Events)
                  .Include(x => x.JobOrder)
                 .Where(x => (x.JobOrder.CompanyId == user.CompanyId) && (x.Id == inv.assessmentid))
                 .SingleOrDefaultAsync();

                if(query==null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, result = "No records found."  });
                }
                var dictionary = new Dictionary<string, string>();
                foreach (Event evt in query.Calendar.Events)
                {
                    dictionary.Add(evt.Title, await _helperService.EmailiCal(evt.Id));
                }
                _context.Entry<Calendar>(query.Calendar).State = EntityState.Detached;
                Calendar cal = new Calendar();
                cal.Id = query.Calendar.Id;
                _context.Calendar.Attach(cal);
                cal.InviteSent = true;
                cal.SentBy = user.Id;
                cal.SentOn = DateTime.Now;
                await _context.SaveChangesAsync();

                if (query.CalendarId == null)
                {
                    Calendar cal2 = new Calendar();
                    cal2.AddedOn = DateTime.Now;
                    _context.Calendar.Add(cal);
                    await _context.SaveChangesAsync();

                    Assessment Ast = new Assessment();
                    Ast.Id = query.Id;
                    _context.Assessment.Attach(Ast);
                    Ast.CalendarId = cal2.Id;
                    Ast.AssessmentStatusId = 11;
                    await _context.SaveChangesAsync();
                    _context.Entry<Calendar>(cal2).State = EntityState.Detached;
                    _context.Entry<Assessment>(Ast).State = EntityState.Detached;

                }


                return await Assessment2(inv.assessmentid);

            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var evt = await _context.Event.Include(p => p.Participants).SingleOrDefaultAsync(p => p.Id == id);
                if (evt == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                _context.EventUser.RemoveRange(evt.Participants.ToList());
                _context.Event.Remove(evt);

                await _context.SaveChangesAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListInterviews([FromQuery]Paging pg)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.Event
               .Where(x => (x.Companyid == user.CompanyId) && (x.EventType == 2))
               .Where(p=>p.Participants.Any(e=>e.ParticipantId==user.Id))
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
                   assesmentid = _context.Assessment.Where(a => a.CalendarId == x.CalendarId).Select(i => i.Id).SingleOrDefault(),
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
               });

                query = pg.q != "" ? query.Where(x => (x.subject.Contains(pg.q)) || (x.description.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnBoarding(int assessmentid)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.Assessment
                    .Include(x => x.JobOrder)
                    .ThenInclude(c=>c.Company)
                   .Where(x => (x.JobOrder.CompanyId == user.CompanyId) && (x.Id == assessmentid))
                   .Select(x => new
                   {
                       assessmentid = x.Id,
                       rating = x.TotalRating,
                       updatedon = x.UpdatedOn,
                       Candidate = _context.Candidate.Include(c => c.User).Where(c => c.Id == x.CandidateId).Select(c => new { id = c.Id, name = c.Name, position = c.Position, skills = c.Skills, userid = c.UserId, photo = _helperService.GetUserPhoto(c.User) }).SingleOrDefault(),
                       onBoarding = _context.AssessmentOnBoarding.Where(o => o.Id == x.OnBoardingId).Select(o=> new {
                           id = o.Id,
                           joindate = o.JoiningDate,
                           message = o.Message,
                           filename = _helperService.GetMediaUrl(x.JobOrder.Company.UID.ToString()) + "/" + o.FilePath,
                           addedbyid = o.AddedById,
                           addedby = o.AddedBy.FullName,
                           status = o.StatusId,
                           addedon = o.AddedOn
                       }).SingleOrDefault()
                   })
                 .SingleOrDefaultAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> SaveOnboarding([FromForm]onboarding offer)
        {
            var file = Request.Form.Files[0];
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var cuid = await _context.Company.Where(x => x.Id == user.CompanyId).Select(c => c.UID).SingleOrDefaultAsync();
                string fileName = "";
                string filepath = "";
                if (file.Length > 0)
                {
                    fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    FileInfo fi = new FileInfo(fileName);
                    if (fi.Extension.ToLower() == ".doc" || fi.Extension.ToLower() == ".docx" || fi.Extension.ToLower() == ".pdf")
                    {
                        string fileN = _helperService.RandomString(8, true) + fi.Extension;
                        string folder = _helperService.GetMediaPath() +"\\"+ cuid.ToString();
                        string dr = Directory.GetCurrentDirectory();
                        string savePath = Path.Combine(Directory.GetParent(dr).Parent.ToString(), folder);
                        if (!Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        string fullPath = Path.Combine(savePath, fileN);
                        filepath = fileN;
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }


                var asmodel = await _context.Assessment.Include(x => x.OnBoarding).Where(x => x.Id ==offer.AssesmentId).SingleOrDefaultAsync();
                AssessmentOnBoarding ob = new AssessmentOnBoarding();
                if (asmodel.OnBoardingId ==null)
                {
                    if(!string.IsNullOrEmpty(filepath))
                    {
                        ob.FilePath = filepath;
                    }

                    ob.Message = offer.Message;
                    ob.JoiningDate = offer.JoiningDate;
                    ob.StatusId = offer.StatusId;
                    ob.AddedById = user.Id;
                    ob.AddedOn = DateTime.Now;
                    await _context.AssessmentOnBoarding.AddAsync(ob);
                    asmodel.OnBoardingId =ob.Id;
                }
                else
                {
                    _context.Entry<AssessmentOnBoarding>(asmodel.OnBoarding).State = EntityState.Detached;
                    if (!string.IsNullOrEmpty(filepath))
                    {
                        ob.FilePath = filepath;
                    }
                    ob.Id = offer.Id;
                    _context.AssessmentOnBoarding.Attach(ob);
                    ob.Message = offer.Message;
                    ob.JoiningDate = offer.JoiningDate;
                    ob.StatusId = offer.StatusId;
                    ob.AddedById = user.Id;
                    ob.AddedOn = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                StatusDTO sto = new StatusDTO();
                sto.assessmentId = offer.AssesmentId;
                sto.statusId = 15;
                await UpdateStatus(sto);
                return await OnBoarding(offer.AssesmentId);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Candidates([FromQuery]Paging pg)
        {

            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                //                var joborder = _context.JobOrder.Where(x => x.Id == joborderid).SingleOrDefault();
                //               var skillset = joborder.Skills.Split(',');
                var query = _context.Candidate
                    .Include(a=>a.Address)
                    .ThenInclude(c=>c.City)
                    .ThenInclude(s=>s.State)
                // .Where(c => skillset.Any(j=>c.Skills.Contains(j)));
                .Where(x => (x.CompanyId == comp.Id))
                 .Select(c => new {
                     id = c.Id,
                     name = c.Name,
                     email =c.Email,
                     position = c.Position,
                     skills = c.Skills,
                     userid = c.UserId,
                     experience = c.Experience,
                     photo = _helperService.GetUserPhoto(c.User),
                     rating = c.Rating, updated=c.Updated,
                     address = c.Address.AddressLine,
                     city = c.Address.CityId,
                     state=c.Address.City.State,
                     zipcode = c.Address.ZipCode,
                     phone = c.Address.Phone
                     
                 });
                query = pg.q != "" ? query.Where(x => (x.name.Contains(pg.q)) || (x.position.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.name) : query = query.OrderBy(w => w.position); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<ActionResult> CandidateDetail(int candidateid)
        {

            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.Candidate
                     .Include(a => a.Address)
                    .ThenInclude(c => c.City)
                    .ThenInclude(s => s.State)
                .Include(u=>u.User)
                .Where(x => (x.CompanyId == comp.Id) && (x.Id==candidateid))
                 .Select(c => new {
                     id = c.Id,
                     name = c.Name,
                     email = c.Email,
                     position = c.Position,
                     skills = c.Skills,
                     userid = c.UserId,
                     experience = c.Experience,
                     photo = _helperService.GetUserPhoto(c.User),
                     rating = c.Rating,
                     updated = c.Updated,
                     address = c.Address.AddressLine,
                     city = c.Address.CityId,
                     state = c.Address.City.State,
                     zipcode = c.Address.ZipCode,
                     phone = c.Address.Phone

                 })
                .SingleOrDefaultAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveCandidate([FromBody]candidate cdata)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var tcand = _context.Candidate.Include(x => x.Address).SingleOrDefault(x => x.Id == cdata.Id);
                if (tcand != null)
                {
                    _context.Entry<Candidate>(tcand).State = EntityState.Detached;
                    if(tcand.Address!=null)
                    _context.Entry<Address>(tcand.Address).State = EntityState.Detached;
                }

                Candidate ca = new Candidate();
                if (cdata.Id == 0)
                {
                    ca.Name = cdata.Name;
                    ca.Email = cdata.Email;
                    ca.Position = cdata.Position;
                    ca.Rating = cdata.Rating;
                    ca.Education = cdata.Education;
                    ca.Experience = cdata.Experience;
                    ca.Skills = cdata.Skills;
                    ca.DOB = cdata.DOB;
                    ca.Twitter = cdata.Twitter;
                    ca.LinkedIn = cdata.LinkedIn;
                    ca.Created = DateTime.Now;
                    ca.Updated = DateTime.Now;
                    ca.AddedBy = ca.UserId;
                    ca.CompanyId = user.CompanyId;
                   
                        Address cadrr = new Address();
                        cadrr.AddressLine = cdata.Address;
                        if (cdata.CityId != 0)
                        {
                            cadrr.CityId = cdata.CityId;
                        }
                        cadrr.Phone = cdata.Phone;
                        cadrr.ZipCode = cdata.ZipCode;
                        _context.Address.Add(cadrr);
                        ca.Addressid = cadrr.id;
                         _context.Candidate.Add(ca);
                         await _context.SaveChangesAsync();
                }
                else
                {
                    //First update address

                        Address cadrr = new Address();
                        cadrr.id = tcand.Address.id;
                        _context.Address.Attach(cadrr);
                        cadrr.AddressLine = cdata.Address;
                        if (cdata.CityId != 0)
                        {
                            cadrr.CityId = cdata.CityId;
                        }
                        cadrr.Phone = cdata.Phone;
                        cadrr.ZipCode = cdata.ZipCode;
//                    _context.Entry<Address>(cadrr).State = EntityState.Detached;

                    //Detact address
                    ca.Id = cdata.Id;
                    _context.Candidate.Attach(ca);
                    ca.Address = cadrr;
                    ca.Name = cdata.Name;
                    ca.Email = cdata.Email;
                    ca.Position = cdata.Position;
                    ca.Rating = cdata.Rating;
                    ca.Education = cdata.Education;
                    ca.Experience = cdata.Experience;
                    ca.Skills = cdata.Skills;
                    ca.DOB = cdata.DOB;
                    ca.Twitter = cdata.Twitter;
                    ca.LinkedIn = cdata.Twitter;
                    ca.Updated = DateTime.Now;
                    // 
                }
                await _context.SaveChangesAsync();

                return await CandidateDetail(ca.Id);

            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidate(int id)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var candidate = await _context.Candidate.FindAsync(id);
                if (candidate == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                _context.Candidate.Remove(candidate);
                await _context.SaveChangesAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Deleted Successfully" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCandidates([FromBody] int[] ids)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                foreach (int i in ids)
                {
                    var query = await _context.Candidate.FindAsync(i);
                    if (query == null)
                    {
                        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                    }
                    _context.Candidate.Remove(query);
                }
                await _context.SaveChangesAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Deleted Successfully" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> QTypes([FromQuery]Paging pg)
        {

            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.QuestionType
                .Where(x => (x.CompanyId == comp.Id))
                .Select(q => new
                {
                    id = q.Id,
                    typename = q.TypeName,
                    description = q.Description,
                    questions = _context.Question.Where(Q=>Q.QuestionTypeId == q.Id).Select(x=>x.Id).Count()
            });
                query = pg.q != "" ? query.Where(x => (x.typename.Contains(pg.q)) || (x.description.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.typename);

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<ActionResult> QType(int id)
        {
            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var qtype = await _context.QuestionType
                .Where(x => (x.CompanyId == comp.Id) && (x.Id==id))
                .Select(q => new
                {
                    id = q.Id,
                    typename = q.TypeName,
                    description = q.Description,
                    UserId = q.UserId,
                    CompanyId = q.CompanyId
                }).SingleOrDefaultAsync();

               return Ok(new { StatusCode = StatusCodes.Status200OK, result = qtype });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveQType([FromBody]qtype qtdata)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                QuestionType qt = new QuestionType();
                if (qtdata.Id == 0)
                {
                    qt.TypeName = qtdata.TypeName;
                    qt.Description = qtdata.Description;
                    qt.CompanyId = user.CompanyId;
                    qt.UserId = user.Id;
                    _context.QuestionType.Add(qt);
                }
                else
                {
                    qt.Id = qtdata.Id;
                    _context.QuestionType.Attach(qt);
                    qt.TypeName = qtdata.TypeName;
                    qt.Description = qtdata.Description;
                    qt.CompanyId = user.CompanyId;
                    qt.UserId = user.Id;
                }
                await _context.SaveChangesAsync();
                return await QType(qt.Id);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQType(int id)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var qtype = await _context.QuestionType.FindAsync(id);
                if (qtype == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                var ques = await _context.Question.Where(p => p.QuestionTypeId == qtype.Id)
                    .ToListAsync();

                var quesREf = await _context.Question.Join(_context.JobQuestion,
                    question=> question.Id, jobQuestion=> jobQuestion.QuestionId,
                    (question,jobQuestion)=> new { QuestionTypeId = question. QuestionTypeId, QuestionId = question. Id}).Where(p => p.QuestionTypeId == qtype.Id)
                .ToListAsync();
                if(quesREf.Count==0)
                { 
                    _context.Question.RemoveRange(ques.ToList());
                    _context.QuestionType.Remove(qtype);
                }
                else
                {
                    //TODO: should return out side the loop
                    return Ok(new ErrorDto { StatusCode = StatusCodes.Status409Conflict, Message = "This Question type is used in some other job " });
                }

                await _context.SaveChangesAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Questions([FromQuery]Paging pg, int qtypeid)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.Question
                .Where(x => (x.QuestionType.CompanyId == user.CompanyId) && (x.QuestionTypeId == qtypeid))
                .Select(q => new
                {
                    id = q.Id,
                    question = q.QuestionTitle,
                    duration = q.Duration,
                    buffertime = q.BufferTime,
                    description = q.Description,
                });
                query = pg.q != "" ? query.Where(x => (x.question.Contains(pg.q)) || (x.description.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.question);

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Question(int id)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var qtype = await _context.Question
                .Where(x => (x.QuestionType.CompanyId == user.CompanyId) && (x.Id == id))
                .Select(q => new
                {
                    id = q.Id,
                    title = q.QuestionTitle,
                    question = q.QuestionTitle,
                    duration = q.Duration,
                    buffertime = q.BufferTime,
                    description = q.Description,
                    userid = q.UserId,
                    typeid =q.QuestionTypeId,
                    type = q.QuestionType.TypeName,
                    isactive = q.IsActive,
                    updated = q.Updated

                }).SingleOrDefaultAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = qtype });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveQuestion([FromBody]question quesdata)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                Question ques = new Question();
                if (quesdata.Id == 0)
                {
                    ques.QuestionTitle = quesdata.QuestionTitle;
                    ques.Duration = quesdata.Duration;
                    ques.BufferTime = quesdata.BufferTime;
                    ques.Description = quesdata.Description;
                    ques.IsActive = true;
                    ques.QuestionTypeId = quesdata.QuestionTypeId;
                    ques.UserId = user.Id;
                    _context.Question.Add(ques);
                }
                else
                {
                    ques.Id = quesdata.Id;
                    _context.Question.Attach(ques);
                    ques.QuestionTitle = quesdata.QuestionTitle;
                    ques.Duration = quesdata.Duration;
                    ques.BufferTime = quesdata.BufferTime;
                    ques.Description = quesdata.Description;
                    ques.IsActive = true;
                    ques.QuestionTypeId = quesdata.QuestionTypeId;

                    ques.Updated = DateTime.Now;
                    ques.UserId = user.Id;
                }
                await _context.SaveChangesAsync();
                return await Question(ques.Id);

            }
        }
         

           [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var qtype = await _context.Question.FindAsync(id);
                if (qtype == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                //Check the Question reference if it's 1 aloow to remove else show error msg
                int jobQuestionCount = _context.JobQuestion.Count(x => x.QuestionId == id);

                if (jobQuestionCount < 1)
                {
                    _context.Question.Remove(qtype);
                }
                else
                { 
                    return Ok(new ErrorDto { StatusCode = StatusCodes.Status409Conflict, Message = "This Question is used in some other job " });
                }
                await _context.SaveChangesAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteQuestions([FromBody] int[] ids)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                foreach (int i in ids)
                {
                    var query = await _context.Question.FindAsync(i);
                    if (query == null)
                    {
                        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                    }
                    // Check the Question reference if it's 1 aloow to remove else show error msg
                    int jobQuestionCount = _context.JobQuestion.Count(x => x.QuestionId == i);

                    if (jobQuestionCount < 1)
                    {
                        _context.Question.Remove(query);
                    }
                    else
                    {
                        //TODO use array to return 
                        return Ok(new ErrorDto { StatusCode = StatusCodes.Status409Conflict, Message = "This Question is used in some other job" });
                    }
                }
                await _context.SaveChangesAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Deleted Successfully" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> UserList()
        {
            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var userlist = await _context.Users
               .Where(x => (x.CompanyId == comp.Id))
               .Select(x => new
               {
                   id = x.Id,
                   name = x.FullName,
                   email = x.Email,
                   jobassigned = _context.JobOrder.Where(j => j.ManagerId == x.Id).Count(),
                   joindate = x.CreatedOn,
                   isactive = !x.IsDeleted,
                   photo = _helperService.GetUserPhoto(x),
                   phone = x.PhoneNumber,
                   roleid = x.UserRoles.FirstOrDefault().Role.Id,
                   role = x.UserRoles.FirstOrDefault().Role.Name
               }).ToListAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = userlist });
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> ImportCandidates(IFormFile file)
        {
            int Candcnt = 0;
            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                try
                {
                    string fileName = "";
                    if (file.Length > 0)
                    {
                        fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Extension.ToLower() == ".csv")
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
                            using (var reader = new StreamReader(fullPath))
                            {
                                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
                                var csvParser = new CsvParser<ImportCandidate>(csvParserOptions, new CsvCandidateMapping());
                                var records = csvParser.ReadFromFile(fullPath, Encoding.UTF8);
                                var result = records.Select(x => x.Result).ToList();
                             
                                List<Candidate> candlist = new List<Candidate>();
                                foreach (ImportCandidate ic in result)
                                {
                                    if(_context.Candidate.Any(x => x.Email == ic.Email && x.CompanyId == comp.Id))
                                    {


                                    }
                                    else
                                    {
                                        Address addr = new Address
                                        {
                                            AddressLine = ic.Address,
                                            Phone = ic.Phone,
                                            ZipCode = ic.ZipCode
                                        };

                                        if (ic.City != null)
                                        {
                                            var citcxt = await _context.City.Where(x => x.Name == ic.City).SingleOrDefaultAsync();
                                            if (citcxt != null)
                                                addr.CityId = citcxt.Id;
                                        }

                                        Candidate cn = new Candidate
                                        {
                                            Name = ic.Name,
                                            Email = ic.Email,
                                            Position = ic.Position,
                                            Education = ic.Education,
                                            Experience = ic.Experience,
                                            Skills = ic.Skills,
                                            DOB = ic.DOB,
                                            Twitter = ic.Twitter,
                                            LinkedIn = ic.LinkedIn,
                                            Address = addr,
                                            CompanyId = comp.Id
                                        };
                                        candlist.Add(cn);
                                        Candcnt++;
                                    }
                                }
                                _context.Candidate.AddRange(candlist);
                                await _context.SaveChangesAsync();
                                //await _context.BulkInsertAsync(candlist);
                            }
                           
                        }
                        else
                        {
                            return BadRequest(new { StatusCode = StatusCodes.Status200OK, message = "Unsuported file format.", result = fileName });
                        }
                    }
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = Candcnt + " candidate(s) imported", result = fileName });
                }
                catch (System.Exception ex)
                {
                    return BadRequest(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
                }
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> ImportQuestion(IFormFile file)
        {
            int Qcnt = 0;
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                try
                {
                    string fileName = "";
                    if (file.Length > 0)
                    {
                        fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Extension.ToLower() == ".csv")
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
                            using (var reader = new StreamReader(fullPath))
                            {
                                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
                                var csvParser = new CsvParser<ImportQuestion>(csvParserOptions, new CsvQuestionMapping());
                                var records = csvParser.ReadFromFile(fullPath, Encoding.UTF8);
                                var result = records.Select(x => x.Result).ToList();
                             

                               // var Qtyps = result.GroupBy(c => c.Type);
                                IEnumerable<IGrouping<string, ImportQuestion>> Qtyps = result.GroupBy(c => c.Type);
                                foreach (IGrouping<string, ImportQuestion> Ques in Qtyps)
                                {

                                    if (Ques.Key != null)
                                    {
                                        var qtyp =await _context.QuestionType.Where(x => x.TypeName == Ques.Key).SingleOrDefaultAsync();
                                        int qtid = 0;
                                        if (qtyp == null)
                                        {
                                            QuestionType nqtyp = new QuestionType { TypeName = Ques.Key, CompanyId = user.CompanyId, UserId = user.Id };
                                          await  _context.QuestionType.AddAsync(nqtyp);
                                            await  _context.SaveChangesAsync();
                                            qtid = nqtyp.Id;
                                        }
                                        else
                                        {
                                            qtid = qtyp.Id;
                                        }

                                        List<Question> queslist = new List<Question>();
                                            foreach (ImportQuestion q in Ques)
                                            {
                                                Question qs = new Question
                                                {
                                                    QuestionTitle = q.Question,
                                                    Duration = q.Duration,
                                                    BufferTime = q.BufferTime,
                                                    Description = q.Description,
                                                    QuestionTypeId = qtid,
                                                };
                                            queslist.Add(qs);
                                               
                                            }
                                        _context.Question.AddRange(queslist);
                                       await _context.SaveChangesAsync();
                                    }
                                    Qcnt++;
                                }
                            }
                        }
                        else
                        {
                            return BadRequest(new { StatusCode = StatusCodes.Status200OK, message = "Unsuported file format.", result = fileName });
                        }
                    }
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = Qcnt + " question(s) imported", result = fileName });
                }
                catch (System.Exception ex)
                {
                    return BadRequest(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
                }
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> ImportJoborders(IFormFile file)
        {

           
            User user = _helperService.GetUser();
            Company comp = user.Company;
            int jcnt = 0;
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                try
                {
                    string fileName = "";
                    if (file.Length > 0)
                    {
                        fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Extension.ToLower() == ".csv")
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
                            using (var reader = new StreamReader(fullPath))
                            {
                                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
                                var csvParser = new CsvParser<ImportJoborder>(csvParserOptions, new CsvJobOrderMapping());
                                var records = csvParser.ReadFromFile(fullPath, Encoding.UTF8);
                                var result = records.Select(x => x.Result).ToList();
                              
                                List<JobOrder> jolist = new List<JobOrder>();
                                foreach (ImportJoborder imjo in result)
                                {

                                    JobOrder jon = new JobOrder
                                        {
                                            Title = imjo.Title,
                                            Skills = imjo.Skills,
                                            Experience = imjo.Experience,
                                            Openings = imjo.Openings,
                                            Location = imjo.Location,
                                            Summary = imjo.Summary,
                                            Status = 1,
                                            UserId = user.Id,
                                            CompanyId = comp.Id
                                          };

                                    //if (imjo.Company != null)
                                    //{
                                    //    var jocomp = await _context.Company.Where(x => x.Name == imjo.Company).SingleOrDefaultAsync();
                                    //    if (jocomp != null)
                                    //        jon.CompanyId = jocomp.Id;
                                    //    else
                                    //        jon.CompanyId = comp.Id;
                                    //}
                                    //else
                                    //{
                                    //    jon.CompanyId = comp.Id;
                                    //}
                                    
                                    if (imjo.JobType != null)
                                    {
                                        var jotype = await _context.Jobtype.Where(x => x.Type.Trim().ToLower().Replace(" ","") == imjo.JobType.Trim().ToLower().Replace(" ", "")).SingleOrDefaultAsync();
                                        if (jotype != null)
                                            jon.JobTypeId = jotype.Id;
                                    }
                                    if (imjo.RecruiterEmail != null)
                                    {
                                        var jomanager= await _context.User.Where(x => x.Email == imjo.RecruiterEmail).SingleOrDefaultAsync();
                                        if (jomanager != null)
                                            jon.ManagerId = jomanager.Id;
                                    }

                                    jolist.Add(jon);
                                    jcnt++;
                                _context.JobOrder.AddRange(jolist);
                                await _context.SaveChangesAsync();
                                }
                              
                                //await _context.BulkInsertAsync(candlist);
                            }

                        }
                        else
                        {
                            return BadRequest(new { StatusCode = StatusCodes.Status200OK, message = "Unsuported file format.", result = fileName });
                        }
                    }
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = jcnt +" job order(s) imported", result = fileName });
                }
                catch (System.Exception ex)
                {
                    return BadRequest(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
                }
            }
        }

        [HttpGet]
        public async Task<ActionResult> cjobcandidates()
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jobc = await _context.Assessment.Include(x=>x.Candidate)
                      .Where(x => (x.Candidate.CompanyId == user.CompanyId))
                       .GroupBy(c => new { c.Candidate.Id, c.Candidate.Name, c.Candidate.Email})
                       .Select(g => new {id= g.Key.Id, name= g.Key.Name, email= g.Key.Email })
                       .Distinct()
                       .ToListAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobc });
            }

        }
        [HttpGet]
        public async Task<ActionResult> cjoborders(int candidateid)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var job1 = await _context.Assessment.Include(x => x.Candidate)
                      .Where(x => (x.Candidate.CompanyId == user.CompanyId) &&(x.CandidateId == candidateid) && (x.AssessmentStatusId>=1))
                       .GroupBy(c => new { c.JobOrderId, c.JobOrder.Title })
                       .Select(g => new { id = g.Key.JobOrderId, title = g.Key.Title })
                       .Distinct()
                       .ToListAsync();
                var job2 = await _context.JobOrder
                   .Where(x => (x.CompanyId == user.CompanyId)&&x.Status==2)
                    .Select(g => new { id = g.Id, title = g.Title })
                    .Distinct()
                    .ToListAsync();
                job2.RemoveAll(item => job1.Contains(item));
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = job1, result2=job2 });
            }

        }

        [HttpGet]
        public async Task<ActionResult> cjassessments(int joborderid, int candidateid)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jobq = await _context.JobQuestion
                 .Include(x => x.Question)
                .Where(x => (x.Question.QuestionType.CompanyId == user.CompanyId) && x.JobOrderId == joborderid)
                .Select(q => new
                {
                    id = q.Question.Id,
                    question = q.Question.QuestionTitle,
                    duration = q.Question.Duration,
                    description = q.Question.Description,
                    qtypeid = q.Question.QuestionTypeId,
                    qtype = q.Question.QuestionType.TypeName,
                    orderby = q.OrderById
                }).ToListAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobq });
            }
            

        }

        [HttpPost]
        public async Task<IActionResult> saverepsonses(int job1,int job2, int candidateid, jobresponses resp)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var j2 = await _context.Assessment.Include(x => x.Candidate)
                   .Where(x => (x.JobOrderId == job2 && x.CandidateId == candidateid)).FirstOrDefaultAsync();
                if(j2==null)
                {
                    Assessment asmt = new Assessment();


                }
                 
                return await cjoborders(candidateid);

            }
        }


        #region MCQ Question

        [HttpGet]
        public async Task<IActionResult> getDocumentsTypes()
        {
            var documents = await _context.DocumentTemplate
            .Select(x => new
            {
                id = x.Id,
                documentname = x.DocumentName
            })
            .ToListAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = documents });
        }

        [HttpGet]
        public async Task<ActionResult> mcqQuestions()
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                try {

                    var query = await _context.FormTemplate.ToListAsync();

                    return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
                }
                catch(Exception ex)
                {

                }
               
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveMCQuestion([FromBody] question quesdata)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                FormTemplate ques = new FormTemplate();
                if (quesdata.Id == 0)
                {
                    ques.Question = quesdata.QuestionTitle;
                    ques.IsActive = 1;
                    ques.CompanyId = (int)user.CompanyId;
                    ques.CreatedBy = (int)user.Id;
                    ques.CreatedOn = DateTime.Now;
                    _context.FormTemplate.Add(ques);
                }
                else
                {
                    ques.Id = quesdata.Id;
                    _context.FormTemplate.Attach(ques);
                    ques.Question = quesdata.QuestionTitle;
                    ques.IsActive = 1;
                    ques.CompanyId = (int)user.CompanyId;
                    ques.CreatedBy = (int)user.Id;
                    ques.CreatedOn = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return await MCQuestion(ques.Id);

            }

        }

        [HttpGet]
        public async Task<ActionResult> MCQuestion(int id)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var mcq = await _context.FormTemplate
                .Where(x => (x.CompanyId == user.CompanyId) && (x.Id == id))
                .Select(q => new
                {
                    id = q.Id,
                    question = q.Question,
                    companyid = q.CompanyId,
                    createdby = q.CreatedBy,
                    createdon = q.CreatedOn

                }).SingleOrDefaultAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = mcq });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobMCQuestion(int joborderid)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var jobq = await _context.JobMCQuestion
                 .Include(x => x.Question)
                .Where(x => (x.Question.CompanyId == user.CompanyId) && x.JobOrderId == joborderid)
                .Select(q => new
                {
                    id = q.Question.Id,
                    question = q.Question.Question,
                    orderby = q.OrderById
                }).ToListAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobq });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MCQuestions(int joborderid, jobquestions questions)
        {

            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var ques = await _context.JobMCQuestion.Where(p => p.JobOrderId == joborderid)
                       .ToListAsync();

                if (ques != null&&ques.Count>0)
                {
                    _context.JobMCQuestion.RemoveRange(ques);
                }

                var Jq = await _context.JobMCQuestion
               .Where(x => x.JobOrderId == joborderid).ToListAsync();

                List<JobMCQuestion> jqlist = new List<JobMCQuestion>();
                foreach (Ques jq in questions.Questions.Distinct().ToArray())
                {
                    JobMCQuestion jqtion = new JobMCQuestion();
                    jqtion.QuestionId = jq.QuestionId;
                    jqtion.OrderById = jq.OrderById;
                    jqtion.AddedById = user.Id;
                    jqtion.AddedOn = DateTime.Now;
                    jqtion.JobOrderId = joborderid;
                    jqlist.Add(jqtion);
                }
                
                await _context.JobMCQuestion.AddRangeAsync(jqlist);
                await _context.SaveChangesAsync();

                return await JobMCQuestion(joborderid);
            }
        }

        #endregion


    }
}
