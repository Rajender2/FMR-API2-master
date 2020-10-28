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
        public class JobQuestionController : ControllerBase
        {
            private readonly DataContext _context;
            private readonly IHelperService _helperService;

            public JobQuestionController(DataContext context, IHelperService helperService)
            {
                _context = context;
                _helperService = helperService;
            }

            // GET: api/JobQuestions
            [HttpGet]
            public async Task<ActionResult<IEnumerable<JobQuestion>>> GetJobQuestions()
            {
                var JobQuestions = await _context.JobQuestion.ToListAsync();
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = JobQuestions });
            }


            // GET: api/JobQuestions/5
            [HttpGet("{id}")]
            public async Task<ActionResult<JobQuestion>> GetJobQuestions(int id)
            {
                var JobQuestions = await _context.JobQuestion.FindAsync(id);
                if (JobQuestions == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }
                return Ok(new { StatusCode = StatusCodes.Status200OK, result = JobQuestions });
            }

            //PUT: api/JobQuestions/5
            [HttpPut("{id}")]
            public async Task<ActionResult<JobQuestion>> PutJobQuestions(int id, JobQuestion JobQuestions)
            {
                if (id != JobQuestions.Id)
                {
                    return BadRequest();
                }
                _context.Entry(JobQuestions).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobQuestionsExists(id))
                    {
                        return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = JobQuestions });

            }

            [HttpPatch("{id}")]
            public async Task<ActionResult<JobQuestion>> Patch(int id, [FromBody]JsonPatchDocument<JobQuestion> JobQuestion)
            {
                var JobQuestionDB = await _context.JobQuestion.FindAsync(id);
                JobQuestion.ApplyTo(JobQuestionDB);
                return JobQuestionDB;
            }
            // POST: api/JobQuestions
            [HttpPost]
            public async Task<ActionResult<JobQuestion>> PostJobQuestions(JobQuestion JobQuestions)
            {
                _context.JobQuestion.Add(JobQuestions);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetJobQuestions", new { id = JobQuestions.Id }, JobQuestions);
            }

            // DELETE: api/JobQuestions/5
            [HttpDelete("{id}")]
            public async Task<ActionResult<JobQuestion>> DeleteJobQuestions(int id)
            {
                var JobQuestions = await _context.JobQuestion.FindAsync(id);
                if (JobQuestions == null)
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
                }

                _context.JobQuestion.Remove(JobQuestions);
                await _context.SaveChangesAsync();

                return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
            }

            private bool JobQuestionsExists(int id)
            {
                return _context.JobQuestion.Any(e => e.Id == id);
            }
        }
    }

