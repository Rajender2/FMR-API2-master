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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.IO;


namespace WebApi.Controllers
{
    [Route("[controller]/[Action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class CompanyController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHelperService _helperService;
        private RoleManager<Role> _RoleManager;
        private UserManager<User> _UserManager;

        public CompanyController(DataContext context, IHelperService helperService, IServiceProvider serviceProvider)
        {
            _context = context;
            _helperService = helperService;
            _RoleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            _UserManager = serviceProvider.GetRequiredService<UserManager<User>>();
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            DashboardDTO dashboard = new DashboardDTO();
            ChartData newJobOrder = new ChartData();
            newJobOrder.Name = "New";
            ChartData active = new ChartData();
            active.Name = "Active";
            ChartData closed = new ChartData();
            closed.Name = "Closed";
            var qdata = "";
            try
            {
                var query = from jo in _context.JobOrder
                            group jo by jo.Status into jog
                            select new
                            {
                                Status = jog.Key,
                                Count = jog.Count()
                            };
                foreach(var data in query)
                {
                    switch (data.Status)
                    {
                        case 1:
                            newJobOrder.Data.Add(data.Count);
                            break;

                        case 2:
                            active.Data.Add(data.Count);
                            break;
                        case 3:
                            closed.Data.Add(data.Count);
                            break;
                    }
                }
                dashboard.JobOrders.Add(newJobOrder);
                dashboard.JobOrders.Add(active);
                dashboard.JobOrders.Add(closed);


                ChartData applied = new ChartData();
                applied.Name = "Applied";
                ChartData screening = new ChartData();
                screening.Name = "Screening";
                ChartData shortlisted = new ChartData();
                shortlisted.Name = "Shortlisted";
                ChartData onboarding = new ChartData();
                onboarding.Name = "OnBoarding";

                var assessment = from assmnt in _context.Assessment where new List<int> { 1, 2, 3, 12 }.Contains(assmnt.AssessmentStatusId.Value)
                                 group assmnt by assmnt.AssessmentStatusId into assmntg
                                 select new
                            {
                                Status = assmntg.Key,
                                Count = assmntg.Count()
                            };
                foreach (var data in assessment)
                {
                    switch (data.Status)
                    {
                        case 1:
                            applied.Data.Add(data.Count);
                            break;

                        case 2:
                            screening.Data.Add(data.Count);
                            break;
                        case 3:
                            shortlisted.Data.Add(data.Count);
                            break;
                        case 12:
                            onboarding.Data.Add(data.Count);
                            break;
                    }
                }
                dashboard.CandidateResponses.Add(applied);
                dashboard.CandidateResponses.Add(screening);
                dashboard.CandidateResponses.Add(shortlisted);
                dashboard.CandidateResponses.Add(onboarding);

                dashboard.JobOrdersCount = dashboard.JobOrders.Sum(x=>x.Data.Count);
                dashboard.InterviewsCount = dashboard.CandidateResponses.Sum(x => x.Data.Count);
                Company comp = _helperService.GetCompany();
                if (comp != null)
                {
                    dashboard.RecruitersCount = _context.User.Where(x =>x.CompanyId==comp.Id&& x.UserRoles.Any(y => y.RoleId.Equals(3))).ToList().Count;
                }
                
                
            }
            catch
            {

            }
            

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = dashboard });
        }

        [HttpGet]
        public async Task<IActionResult> Joborders([FromQuery]Paging pg)
        {

            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.JobOrder
                 .Where(x => x.CompanyId == comp.Id)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    jobtypeid = x.JobType.Id,
                    jobtype = x.JobType.Type,
                    //  location = x.Company.Address.City.Name + ", " + x.Company.Address.City.State.Code,
                    location = x.Location,
                    companyname = x.CompanyName,
                    experience = x.Experience,
                    openings = x.Openings,
                    skills = x.Skills,
                    managerid = x.ManagerId,
                    manager = x.Manager.FullName,
                    status = x.Status,
                    summary = x.Summary,
                    candidatecount=x.Assessments.Count
                    
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
        public async Task<IActionResult> Joborder(int id)
        {

            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var joborder = await _context.JobOrder
                 .Where(x => x.CompanyId == comp.Id && x.Id== id)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    jobtypeid = x.JobType.Id,
                    jobtype = x.JobType.Type,
                    //location = x.Company.Address.City.Name + ", " + x.Company.Address.City.State.Code,
                    location = x.Location,
                    companyname = x.CompanyName,
                    experience = x.Experience,
                    openings = x.Openings,
                    skills = x.Skills,
                    managerid = x.ManagerId,
                    manager = x.Manager.FullName,
                    status = x.Status,
                    summary = x.Summary
                }).SingleOrDefaultAsync();
             
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = joborder });
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
                    jb.ManagerId = job.ManagerId;
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
                return await Joborder(jb.Id);
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

                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Deleted Successfully", joborder=jobOrder });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteJobOrders([FromBody] int[] ids)
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
                    var jobOrder = await _context.JobOrder.FindAsync(i);
                    if (jobOrder == null)
                    {
                        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                    }
                    _context.JobOrder.Remove(jobOrder);
                }
                await _context.SaveChangesAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "Deleted Successfully" });
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
                .Where(x => (x.CompanyId == user.CompanyId) && (x.Status == 2))
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
                    published = x.Published,
                    updated = x.Updated,
                    status = x.Status,
                    applied = x.Assessments.Where(a => a.AssessmentStatusId == 1).Count(),
                    screening = x.Assessments.Where(a => a.AssessmentStatusId == 2).Count(),
                    shortlisted = x.Assessments.Where(a => a.AssessmentStatusId == 3).Count(),
                    onboarding = x.Assessments.Where(a => a.AssessmentStatusId == 12).Count()

                });

                query = pg.q != "" ? query.Where(x => (x.title.Contains(pg.q)) || (x.location.Contains(pg.q)) || (x.skills.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id); 

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Assessment2(int joborderid,string code)
        {

            User user = _helperService.GetUser();
            if (user == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                code = code == "null" ? "" : code;
                if (!string.IsNullOrEmpty(code))
                {
                    var query = await _context.JobOrder
                      .Include(x => x.Assessments).ThenInclude(x => x.Candidate)
                      .Where(x => (x.CompanyId == user.CompanyId)  && (x.InviteId == code))
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
                         assessments = x.Assessments.Where(a => (a.AssessmentStatusId >= 6)).OrderByDescending(o => o.UpdatedOn)
                    .Select(a => new { assessmentid = a.Id, status = a.AssessmentStatusId, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                         sellists = x.Assessments.Where(a => ((a.Batch != null || a.Batch != 0) && a.AssessmentStatusId < 6)).OrderBy(o => o.OrderById)
                    .Select(a => new { assessmentid = a.Id, status = a.AssessmentStatusId, batch = a.Batch, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                     }).SingleOrDefaultAsync();

                    return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });

                }
                else 
                {
                    var query = await _context.JobOrder
                     .Include(x => x.Assessments).ThenInclude(x => x.Candidate)
                    .Where(x => (x.CompanyId == user.CompanyId) && (x.Status == 2) && (x.Id == joborderid))
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
                        assessments = x.Assessments.Where(a => (a.AssessmentStatusId >= 6)).OrderByDescending(o => o.UpdatedOn)
                    .Select(a => new { assessmentid = a.Id, status = a.AssessmentStatusId, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                        sellists = x.Assessments.Where(a => ((a.Batch != null || a.Batch != 0) && a.AssessmentStatusId < 6)).OrderBy(o => o.OrderById)
                    .Select(a => new { assessmentid = a.Id, status = a.AssessmentStatusId, batch = a.Batch, rating = a.TotalRating, uploadedon = a.UpdatedOn, candidate = new { id = a.Candidate.Id, name = a.Candidate.Name, position = a.Candidate.Position, skills = a.Candidate.Skills, userid = a.Candidate.UserId, photo = _helperService.GetUserPhoto(a.Candidate.User) } }),
                    }).SingleOrDefaultAsync();

                    return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
                }
             
            }
          
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody]StatusDTO status)
        {
            User user = _helperService.GetUser();
            if (user.CompanyId == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
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
                return await Assessment2(assmnt.JobOrderId,"");
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
                 .Where(x => (x.CompanyId == user.CompanyId) && (x.Status == 2) && (x.Id == joborderid))
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
                    .Include(x => x.Candidate).ThenInclude(x => x.User)
                    .Include(x => x.JobOrder)
                   .Where(x => (x.JobOrder.CompanyId == user.CompanyId) && (x.Id == assessmentid))
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
        }

        [HttpGet]
        public async Task<IActionResult> Hiring([FromQuery]Paging pg)
        {
            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = _context.Users
               .Where(x => (x.CompanyId == comp.Id) && (x.UserRoles.FirstOrDefault().RoleId == 3))
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
               });
                query = pg.q != "" ? query.Where(x => (x.name.Contains(pg.q)) || (x.email.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id);

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging Paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = Paged, result = qdata });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Manager(long id)
        {
            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var query = await _context.Users
               .Where(x => (x.CompanyId == comp.Id) && (x.Id==id))
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
               }).SingleOrDefaultAsync();
           
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = query });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddManager(manager manager)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                //var Usr = await _helperService.AddUser(user.CompanyId, manager.Name, manager.Email, manager.Password, manager.Phone, "MANAGER", false);
                if (!_context.User.Any(u => u.Email == manager.Email.ToLower()))
                {
                    var uguid = Guid.NewGuid();
                    User usr = new User { CompanyId = user.CompanyId, UserGuid = uguid, UserName = manager.Email.ToLower(), FullName = manager.Name, Email = manager.Email.ToLower(), PhoneNumber = manager.Phone };

                    var createUser = await _UserManager.CreateAsync(usr, manager.Password);
                    if (createUser.Succeeded)
                    {
                        _helperService.GenAvatar(uguid.ToString(), usr.FullName);
                        await _UserManager.AddToRoleAsync(usr, "MANAGER");
                        string token = await _UserManager.GenerateEmailConfirmationTokenAsync(usr);
                        var result = await _UserManager.ConfirmEmailAsync(usr, token);
                        if (result.Succeeded)
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status400BadRequest, Message = "Password policy not met" });
                    }
                    return await Manager(usr.Id);
                }
                else
                {
                    return BadRequest(new ErrorDto { StatusCode = StatusCodes.Status400BadRequest, Message = "Email id already exists" });
                }

                

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
                       id=p.ParticipantId,
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

                if (query == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, result = "No records found." });
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

                return await Interviews(inv.assessmentid);

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
               .Where(p => p.Participants.Any(e => e.ParticipantId == user.Id))
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
                    .ThenInclude(c => c.Company)
                   .Where(x => (x.JobOrder.CompanyId == user.CompanyId) && (x.Id == assessmentid))
                   .Select(x => new
                   {
                       assessmentid = x.Id,
                       rating = x.TotalRating,
                       updatedon = x.UpdatedOn,
                       Candidate = _context.Candidate.Include(c => c.User).Where(c => c.Id == x.CandidateId).Select(c => new { id = c.Id, name = c.Name, position = c.Position, skills = c.Skills, userid = c.UserId, photo = _helperService.GetUserPhoto(c.User) }).SingleOrDefault(),
                       onBoarding = _context.AssessmentOnBoarding.Where(o => o.Id == x.OnBoardingId).Select(o => new {
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
                        string folder = _helperService.GetMediaPath() + "\\" + cuid.ToString();
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


                var asmodel = await _context.Assessment.Include(x => x.OnBoarding).Where(x => x.Id == offer.AssesmentId).SingleOrDefaultAsync();
                AssessmentOnBoarding ob = new AssessmentOnBoarding();
                if (asmodel.OnBoardingId == null)
                {
                    if (!string.IsNullOrEmpty(filepath))
                    {
                        ob.FilePath = filepath;
                    }

                    ob.Message = offer.Message;
                    ob.JoiningDate = offer.JoiningDate;
                    ob.StatusId = offer.StatusId;
                    ob.AddedById = user.Id;
                    ob.AddedOn = DateTime.Now;
                    await _context.AssessmentOnBoarding.AddAsync(ob);
                    asmodel.OnBoardingId = ob.Id;
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
                return await OnBoarding(offer.AssesmentId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reschedule(evnt edata)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {

                Event evt = new Event();
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
                                    }), x => x.ParticipantId,true);


                await _context.SaveChangesAsync();

                return await Event(evt.Id);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetOffer(int id)
        {
            Company comp = _helperService.GetCompany();
            if (comp == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var Offer = await _context.Assessment
                    .Include(x=>x.OnBoarding)
                    .Include(x=>x.JobOrder)
                .Where(x => (x.JobOrder.CompanyId == comp.Id) && (x.OnBoarding.Id == id))
                .Select(x=>new
                {
                    id = x.OnBoarding.Id,
                    message = x.OnBoarding.Message,
                    filepath = _helperService.GetMediaUrl(comp.UID.ToString()) + "/" + x.OnBoarding.FilePath,
                    addedon = x.OnBoarding.AddedOn,
                    statusid = x.OnBoarding.StatusId,
                    addedbyid= x.OnBoarding.AddedById
                })
                .SingleOrDefaultAsync();
             
                return Ok(new { StatusCode = StatusCodes.Status200OK,result = Offer });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOffer(IFormFile file, onboarding offer)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                AssessmentOnBoarding ob = new AssessmentOnBoarding();
                ob.Id = offer.Id;
                _context.AssessmentOnBoarding.Attach(ob);

                ob.Message = offer.Message;
                ob.FilePath = offer.FilePath;
                ob.JoiningDate = offer.JoiningDate;
                ob.StatusId = offer.StatusId;
                ob.AddedById = user.Id;
                ob.AddedOn = DateTime.Now;

                await _context.SaveChangesAsync();
                return await GetOffer(ob.Id);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOffer(int Id)
        {
            User user = _helperService.GetUser();
            if (user == null)
            {
                return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            }
            else
            {
                var ab = await _context.AssessmentOnBoarding.FindAsync();
                if (ab == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }

                _context.AssessmentOnBoarding.Remove(ab);
                await _context.SaveChangesAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
            }
        }
    }
}
