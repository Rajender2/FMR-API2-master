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
    public class AddressController : ControllerBase
    {
        private readonly DataContext _context;

        public AddressController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Addresss
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresss()
        {
            var Addresss = await _context.Address.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Addresss });
        }

        // GET: api/Addresss/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddresss(int id)
        {
            var Addresss = await _context.Address.Include(x=> x.City).Include(x => x.State).Where(x=> x.id == id).SingleOrDefaultAsync();
            if (Addresss == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Addresss });
        }

        //PUT: api/Addresss/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Address>> PutAddresss(int id, Address Addresss)
        {
            if (id != Addresss.id)
            {
                return BadRequest();
            }
            _context.Entry(Addresss).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AddresssExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Addresss });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Address>> Patch(int id, [FromBody]JsonPatchDocument<Address> Address)
        {
            var AddressDB = await _context.Address.FindAsync(id);
            Address.ApplyTo(AddressDB);
            return AddressDB;
        }
        // POST: api/Addresss
        [HttpPost]
        public async Task<ActionResult<Address>> PostAddresss(Address Addresss)
        {
            _context.Address.Add(Addresss);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAddresss", new { id = Addresss.id }, Addresss);
        }

        // DELETE: api/Addresss/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Address>> DeleteAddresss(int id)
        {
            var Addresss = await _context.Address.FindAsync(id);
            if (Addresss == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.Address.Remove(Addresss);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool AddresssExists(int id)
        {
            return _context.Address.Any(e => e.id == id);
        }
    }
}
