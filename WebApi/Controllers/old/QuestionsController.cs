using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class QuestionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHelperService _helperService;

        public QuestionController(DataContext context, IHelperService helperService)
        {
            _context = context;
            _helperService = helperService;
        }

        // GET: api/Questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            var Questions = await _context.Question.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Questions });
        }

        [Route("Type")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> Type([FromQuery] int Id)
        {
            var Questions = await _context.Question.Where(x=> x.QuestionTypeId == Id).ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Questions });
        }
        // GET: api/Questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestions(int id)
        {
            var Questions = await _context.Question.FindAsync(id);
            if (Questions == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Questions });
        }

        //PUT: api/Questions/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Question>> PutQuestions(int id, Question Questions)
        {
            if (id != Questions.Id)
            {
                return BadRequest();
            }
            _context.Entry(Questions).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionsExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Questions });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Question>> Patch(int id, [FromBody]JsonPatchDocument<Question> Question)
        {
            var QuestionDB = await _context.Question.FindAsync(id);
            Question.ApplyTo(QuestionDB,ModelState);
            QuestionDB.Updated = DateTime.Now;
            await _context.SaveChangesAsync();
            return QuestionDB;
        }
        // POST: api/Questions
        [HttpPost]
        public async Task<ActionResult<Question>> PostQuestions(Question Questions)
        {
            _context.Question.Add(Questions);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestions", new { id = Questions.Id }, Questions);
        }

        // DELETE: api/Questions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Question>> DeleteQuestions(int id)
        {
            var Questions = await _context.Question.FindAsync(id);
            if (Questions == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.Question.Remove(Questions);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool QuestionsExists(int id)
        {
            return _context.Question.Any(e => e.Id == id);
        }

        [Route("ImportData")]
      //  [Authorize]
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult UploadCSV()
        {
            try
            {
                var file = Request.Form.Files[0];
                if (file.Length > 0)
                {
                    string foldpath = _helperService.RandomString(8, true);
                    string newPath = _helperService.GetUploads();
    
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "File Uploded Successfully"});
            }
            catch (System.Exception ex)
            {
                return Ok(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
            }
        }
    }
}
