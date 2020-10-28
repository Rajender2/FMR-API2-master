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
    public class EventController : ControllerBase
    {
        private readonly DataContext _context;

        public EventController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var Events = await _context.Event.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Events });
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvents(int id)
        {
            var Events = await _context.Event.Where(x => x.Id == id)
                .Include(x => x.Participants)
                .ThenInclude(x=>x.Participant)
                .SingleOrDefaultAsync();
            if (Events == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Events });
        }

        [Route("Interviews")]
        [HttpGet]
        public async Task<ActionResult<Event>> Interviews()
        {
            var Events = await _context.Event.Where(x => x.EventType==3)
                .Include(x => x.Participants)
                .ThenInclude(x => x.Participant)
                .ToListAsync();
            if (Events == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Events });
        }

        //PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Event>> PutEvents(int id, Event Events)
        {
            if (id != Events.Id)
            {
                return BadRequest();
            }
            _context.Entry(Events).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventsExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Events });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Event>> Patch(int id, [FromBody]JsonPatchDocument<Event> Event)
        {
            var EventDB = await _context.Event.FindAsync(id);
            Event.ApplyTo(EventDB);
            return EventDB;
        }
        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvents(Event Events)
        {
            _context.Event.Add(Events);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvents", new { id = Events.Id }, Events);
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEvents(int id)
        {
            var Events = await _context.Event.FindAsync(id);
            if (Events == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.Event.Remove(Events);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool EventsExists(int id)
        {
            return _context.Event.Any(e => e.Id == id);
        }
    }
}
