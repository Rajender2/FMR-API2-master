using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JobCandidateController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHelperService _helperService;

        public JobCandidateController(DataContext context, IHelperService helperService)
        {
            _context = context;
            _helperService = helperService;
        }

        // GET: api/JobCandidates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobCandidate>>> GetJobCandidates()
        {
            var JobCandidates = await _context.JobCandidate.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = JobCandidates });
        }


        // GET: api/JobCandidates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobCandidate>> GetJobCandidates(int id)
        {
            var JobCandidates = await _context.JobCandidate.FindAsync(id);
            if (JobCandidates == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = JobCandidates });
        }

        //PUT: api/JobCandidates/5
        [HttpPut("{id}")]
        public async Task<ActionResult<JobCandidate>> PutJobCandidates(int id, JobCandidate JobCandidates)
        {
            if (id != JobCandidates.Id)
            {
                return BadRequest();
            }
            _context.Entry(JobCandidates).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobCandidatesExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = JobCandidates });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<JobCandidate>> Patch(int id, [FromBody]JsonPatchDocument<JobCandidate> JobCandidate)
        {
            var JobCandidateDB = await _context.JobCandidate.FindAsync(id);
            JobCandidate.ApplyTo(JobCandidateDB);
            return JobCandidateDB;
        }
        // POST: api/JobCandidates
        [HttpPost]
        public async Task<ActionResult<JobCandidate>> PostJobCandidates(JobCandidate JobCandidates)
        {
            _context.JobCandidate.Add(JobCandidates);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobCandidates", new { id = JobCandidates.Id }, JobCandidates);
        }

        // DELETE: api/JobCandidates/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<JobCandidate>> DeleteJobCandidates(int id)
        {
            var JobCandidates = await _context.JobCandidate.FindAsync(id);
            if (JobCandidates == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
            }

            _context.JobCandidate.Remove(JobCandidates);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool JobCandidatesExists(int id)
        {
            return _context.JobCandidate.Any(e => e.Id == id);
        }

    }
}
