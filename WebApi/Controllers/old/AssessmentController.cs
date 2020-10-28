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

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AssessmentController : ControllerBase
    {
        private readonly DataContext _context;

        public AssessmentController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Assessments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assessment>>> GetAssessment()
        {
            var Assessments = await _context.Assessment.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Assessments });
        }

        // GET: api/Assessments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Assessment>> GetAssessment(int id)
        {
            var Assessments = await _context.Assessment.Where(x=>x.Id == id)
                .Include(x=>x.Candidate)
                .Include(x => x.Responses)
                .ThenInclude(x=>x.Question).SingleOrDefaultAsync();
            if (Assessments == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Assessments });
        }

        [Route("Candidate")]
        [HttpGet]
        public async Task<ActionResult<Assessment>> Candidate(int id)
        {

            var Assessments = await _context.Assessment.Where(x => x.Id == id).SingleOrDefaultAsync();

           await _context.Entry(Assessments).Reference(x => x.Candidate).LoadAsync();
            await _context.Entry(Assessments).Collection(x => x.Responses).LoadAsync();

            if (Assessments == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Assessments });
        }

        [Route("Calendar")]
        [HttpGet]
        public async Task<ActionResult<Assessment>> Calendar(int id)
        {
            var Assessments = await _context.Assessment.Where(x => x.Id == id)
                .Include(x=>x.Candidate)
                .Include(x=>x.Calendar)
                .ThenInclude(x=>x.Events)
                .SingleOrDefaultAsync();

            //await _context.Entry(Assessments).Reference(x => x.Candidate).LoadAsync();
            //await _context.Entry(Assessments).Reference(x => x.Calendar).LoadAsync();
            

            if (Assessments == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Assessments });
        }

        [Route("OnBoarding")]
        [HttpGet]
        public async Task<ActionResult<AssessmentOnboarding>> OnBoarding(int id)
        {
            var Assessments = await _context.Assessment.Where(x => x.Id == id)
                 .Include(x => x.Candidate)
               // .Include(x => x.OnBoarding)
                .SingleOrDefaultAsync();

            //await _context.Entry(Assessments).Reference(x => x.Candidate).LoadAsync();
            //await _context.Entry(Assessments).Reference(x => x.Calendar).LoadAsync();

            if (Assessments == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Assessments });
        }

        //PUT: api/Assessments/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Assessment>> PutAssessment(int id, Assessment Assessments)
        {
            if (id != Assessments.Id)
            {
                return BadRequest();
            }
            _context.Entry(Assessments).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssessmentExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Assessments });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Assessment>> Patch(int id, [FromBody]JsonPatchDocument<Assessment> Assessment)
        {
            var AssessmentDB = await _context.Assessment.FindAsync(id);
            Assessment.ApplyTo(AssessmentDB);
            return AssessmentDB;
        }
        // POST: api/Assessments
        [HttpPost]
        public async Task<ActionResult<Assessment>> PostAssessment(Assessment Assessment)
        {
            _context.Assessment.Add(Assessment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAssessment", new { id = Assessment.Id }, Assessment);
        }

        // DELETE: api/Assessments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Assessment>> DeleteAssessment(int id)
        {
            var Assessment = await _context.Assessment.FindAsync(id);
            if (Assessment == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.Assessment.Remove(Assessment);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool AssessmentExists(int id)
        {
            return _context.Assessment.Any(e => e.Id == id);
        }
    }
}
