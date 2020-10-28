using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;
using X.PagedList;


namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    public class JobOrderController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHelperService _helperService;

        public JobOrderController(DataContext context, IHelperService helperService)
        {
            _context = context;
            _helperService = helperService;
        }

        // GET: api/JobOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobOrder>>> GetJobOrder([FromQuery]Paging pg)
        {

            //Company comp = _helperService.GetCompany();
            //if (comp== null)
            //{
            //    return Ok(new ErrorDto { StatusCode = StatusCodes.Status401Unauthorized, Message = "Unauthorized" });
            //}
            //else
            //{
                var query = _context.JobOrder
                //.Where(x => x.CompanyId == comp.Id)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    jobtypeid = x.JobType.Id,
                    jobtype = x.JobType.Type,
                    location = x.Company.Address.City.Name + ", " + x.Company.Address.State.Code,
                    experience = x.Experience,
                    openings = x.Openings,
                    skills = x.Skills,
                    managerid = x.Manager.Id,
                    manager = x.Manager.FullName,
                    status = x.Status,
                    Company = new { Id = x.Company.Id, Name = x.Company.Name}
                    });
                    
                query = pg.q != "" ? query.Where(x => (x.title.Contains(pg.q)) || (x.Company.Name.Contains(pg.q))) : query;
                query = pg.sort == "dsc" ? query.OrderByDescending(w => w.id) : query = query.OrderBy(w => w.id); ;

                if (pg.size > 100) { pg.size = 25; }

                var qdata = await query.ToPagedListAsync(pg.page, pg.size);

                int totpgs = (int)Math.Ceiling((double)qdata.TotalItemCount / pg.size);
                Paging paged = new Paging { page = pg.page, size = pg.size, totalrecs = qdata.TotalItemCount, totalpages = totpgs, sort = pg.sort };

                return Ok(new { StatusCode = StatusCodes.Status200OK, paged = paged, result = qdata });
            //}
        }

        // GET: api/JobOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobOrder>> GetJobOrder(int id)
        {
            var jobOrders = await _context.JobOrder.Where(x=>x.Id== id)
                .SingleOrDefaultAsync();
            if (jobOrders == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobOrders });
        }

        [Route("Assessments")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobOrder>>> Assessments(int id)
        {
            var jobOrders = await _context.JobOrder.Where(x => x.Id == id)
           .Include(x => x.Assessments).ThenInclude(x => x.Candidate)
           .SingleOrDefaultAsync();
            if (jobOrders == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobOrders });
        }

        
        [Route("Questions")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobOrder>>> Questions(int id)
        {
            var jobOrders = await _context.JobOrder.Where(x => x.Id == id)
            .Include(x => x.Questions)
            .ToListAsync();
            if (jobOrders == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobOrders });
        }


        [Route("Questions")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<JobQuestion>>> Questions(JobQuestion Question)
        {
            _context.JobQuestion.Add(Question);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobOrder", new { id = Question.Id }, Question);
        }
        
        [Route("Questions")]
        [HttpPatch]
        public async Task<ActionResult<JobQuestion>> Questions(int id, [FromBody]JsonPatchDocument<JobQuestion> Question)
        {
          
            var JobQuestionDB = await _context.JobQuestion.FindAsync(id);
            Question.ApplyTo(JobQuestionDB);
            return JobQuestionDB;
        }

        [Route("Questions")]
        [HttpPut]
        public async Task<ActionResult<JobQuestion>> Questions(int id, JobQuestion Question)
        {
            if (id != Question.Id)
            {
                return BadRequest();
            }
            _context.Entry(Question).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobQuestionExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Record Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Question });

        }


        [Route("Candidates")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobOrder>>> Candidates(int id)
        {
            var jobOrders = await _context.JobOrder.Where(x => x.Id == id)
            .Include(x => x.Candidates)
            .ToListAsync();
            if (jobOrders == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobOrders });
        }


        [Route("Candidates")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<JobCandidate>>> Candidates(JobCandidate Candidate)
        {
            _context.JobCandidate.Add(Candidate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobOrder", new { id = Candidate.Id }, Candidate);
        }


        [Route("Candidates")]
        [HttpPatch]
        public async Task<ActionResult<JobCandidate>> Candidates(int id, [FromBody]JsonPatchDocument<JobCandidate> Candidate)
        {

            var JobCandidateDB = await _context.JobCandidate.FindAsync(id);
            Candidate.ApplyTo(JobCandidateDB);
            return JobCandidateDB;
        }

        [Route("Candidates")]
        [HttpPut]
        public async Task<ActionResult<JobCandidate>> Candidates(int id, JobCandidate Candidate)
        {
            if (id != Candidate.Id)
            {
                return BadRequest();
            }
            _context.Entry(Candidate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobCandidateExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Record Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Candidate });

        }


        //PUT: api/JobOrders/5
        [HttpPut("{id}")]
        public async Task<ActionResult<JobOrder>> PutJobOrder(int id, JobOrder jobOrders)
        {
            if (id != jobOrders.Id)
            {
                return BadRequest();
            }
            _context.Entry(jobOrders).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobOrderExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = jobOrders });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<JobOrder>> Patch(int id, [FromBody]JsonPatchDocument<JobOrder> JobOrder)
        {
            var JobOrderDB = await _context.JobOrder.FindAsync(id);
            JobOrder.ApplyTo(JobOrderDB);
            return JobOrderDB;
        }
        // POST: api/JobOrders
        [HttpPost]
        public async Task<ActionResult<JobOrder>> PostJobOrder(JobOrder jobOrder)
        {
            _context.JobOrder.Add(jobOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobOrder", new { id = jobOrder.Id }, jobOrder);
        }

        // DELETE: api/JobOrders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<JobOrder>> DeleteJobOrder(int id)
        {
            var jobOrder = await _context.JobOrder.FindAsync(id);
            if (jobOrder == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.JobOrder.Remove(jobOrder);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool JobOrderExists(int id)
        {
            return _context.JobOrder.Any(e => e.Id == id);
        }

        private bool JobQuestionExists(int id)
        {
            return _context.JobQuestion.Any(e => e.Id == id);
        }
        
        private bool JobCandidateExists(int id)
        {
            return _context.JobCandidate.Any(e => e.Id == id);
        }

    }
}
