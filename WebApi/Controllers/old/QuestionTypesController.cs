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
   public class QuestionTypeController : ControllerBase
    {
        private readonly DataContext _context;

        public QuestionTypeController(DataContext context)
        {
            _context = context;
        }

        // GET: api/QuestionTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionType>>> GetQuestionTypes()
        {
            var QuestionTypes = await _context.QuestionType.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = QuestionTypes });
        }

        // GET: api/QuestionTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionType>> GetQuestionTypes(int id)
        {
            var QuestionTypes = await _context.QuestionType.FindAsync(id);
            if (QuestionTypes == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = QuestionTypes });
        }

        //PUT: api/QuestionTypes/5
        [HttpPut("{id}")]
        public async Task<ActionResult<QuestionType>> PutQuestionTypes(int id, QuestionType QuestionTypes)
        {
            if (id != QuestionTypes.Id)
            {
                return BadRequest();
            }
            _context.Entry(QuestionTypes).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionTypesExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = QuestionTypes });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<QuestionType>> Patch(int id, [FromBody]JsonPatchDocument<QuestionType> QuestionType)
        {
            var QuestionTypeDB = await _context.QuestionType.FindAsync(id);
            QuestionType.ApplyTo(QuestionTypeDB);
            return QuestionTypeDB;
        }
        // POST: api/QuestionTypes
        [HttpPost]
        public async Task<ActionResult<QuestionType>> PostQuestionTypes(QuestionType QuestionTypes)
        {
            _context.QuestionType.Add(QuestionTypes);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestionTypes", new { id = QuestionTypes.Id }, QuestionTypes);
        }

        // DELETE: api/QuestionTypes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<QuestionType>> DeleteQuestionTypes(int id)
        {
            var QuestionTypes = await _context.QuestionType.FindAsync(id);
            if (QuestionTypes == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.QuestionType.Remove(QuestionTypes);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool QuestionTypesExists(int id)
        {
            return _context.QuestionType.Any(e => e.Id == id);
        }
    }
}
