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
    public class CalendarController : ControllerBase
    {
        private readonly DataContext _context;

        public CalendarController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Calendars
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calendar>>> GetCalendar()
        {
            var Calendars = await _context.Calendar.Include(x=>x.Events)
                .ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Calendars });
        }

        // GET: api/Calendars/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Calendar>> GetCalendar(int id)
        {
            var Calendars = await _context.Calendar.Where(x => x.Id == id)
                .Include(x => x.Events)
                .ThenInclude(x=>x.Participants)
                .ThenInclude(x=>x.Participant)
                .SingleOrDefaultAsync();
                
            if (Calendars == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Calendars });
        }

        //PUT: api/Calendars/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Calendar>> PutCalendar(int id, Calendar Calendars)
        {
            if (id != Calendars.Id)
            {
                return BadRequest();
            }
            _context.Entry(Calendars).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CalendarExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Calendars });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Calendar>> Patch(int id, [FromBody]JsonPatchDocument<Calendar> Calendar)
        {
            var CalendarDB = await _context.Calendar.FindAsync(id);
            Calendar.ApplyTo(CalendarDB);
            return CalendarDB;
        }
        // POST: api/Calendars
        [HttpPost]
        public async Task<ActionResult<Calendar>> PostCalendar(Calendar Calendar)
        {
            _context.Calendar.Add(Calendar);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCalendar", new { id = Calendar.Id }, Calendar);
        }

        // DELETE: api/Calendars/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Calendar>> DeleteCalendar(int id)
        {
            var Calendar = await _context.Calendar.FindAsync(id);
            if (Calendar == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.Calendar.Remove(Calendar);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool CalendarExists(int id)
        {
            return _context.Calendar.Any(e => e.Id == id);
        }
    }
}
