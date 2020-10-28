using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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
    public class CandidateController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHelperService _helperService;

        public CandidateController(DataContext context, IHelperService helperService)
        {
            _context = context;
            _helperService = helperService;
        }
        // GET: api/Candidates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidate>>> GetCandidates()
        {
            var Candidates = await _context.Candidate.ToListAsync();
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Candidates });
        }

        // GET: api/Candidates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Candidate>> GetCandidates(int id)
        {
            var Candidates = await _context.Candidate.Where(x=>x.Id == id).SingleOrDefaultAsync();
            if (Candidates == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Candidates });
        }

        //PUT: api/Candidates/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Candidate>> PutCandidates(int id, Candidate Candidates)
        {
            if (id != Candidates.Id)
            {
                return BadRequest();
            }
            _context.Entry(Candidates).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CandidatesExists(id))
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = Candidates });

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<Candidate>> Patch(int id, [FromBody]JsonPatchDocument<Candidate> Candidate)
        {
            var CandidateDB = await _context.Candidate.FindAsync(id);
            Candidate.ApplyTo(CandidateDB);
            return CandidateDB;
        }
        // POST: api/Candidates
        [HttpPost]
        public async Task<ActionResult<Candidate>> PostCandidates(Candidate Candidates)
        {
            _context.Candidate.Add(Candidates);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCandidates", new { id = Candidates.Id }, Candidates);
        }

        // DELETE: api/Candidates/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Candidate>> DeleteCandidates(int id)
        {
            var Candidates = await _context.Candidate.FindAsync(id);
            if (Candidates == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }

            _context.Candidate.Remove(Candidates);
            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = StatusCodes.Status200OK, result = "Deleted Successfully" });
        }

        private bool CandidatesExists(int id)
        {
            return _context.Candidate.Any(e => e.Id == id);
        }


        [Route("ImportData")]
        //  [Authorize]
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult ImportData()
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
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "File Uploded Successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
            }
        }


        [Route("MyJobs")]
        [HttpGet]
        public async Task<ActionResult<vwCandidateMyJobs>> MyJobs(int CandidateId)
        {
            var CandidatemyJob = await _context.vwCandidateMyJobs
               .Where(x => x.CandidateId == CandidateId).ToListAsync();
            if (CandidatemyJob == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message = "No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = CandidatemyJob });
        }


        [Route("ApplyJob")]
        [HttpGet]
        public async Task<ActionResult<vwCandidateJob>> ApplyJob(int JobId, int CandidateId)
        {
            var CandidateJob = await _context.vwCandidateJob
                .Where(x => x.JobOrderId == JobId)
                .Where(x=>x.CandidateId==CandidateId).ToListAsync();
            if (CandidateJob == null)
            {
                return Ok(new { StatusCode = StatusCodes.Status200OK, Message="No Records Found." });
            }
            return Ok(new { StatusCode = StatusCodes.Status200OK, result = CandidateJob });
        }

        [Route("Assessment")]
        //  [Authorize]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult<Candidate>> Assessment(AssesmentCandidate Answer)
        {
            try
            {
                var file = Request.Form.Files[0];
                string foldpath = _helperService.RandomString(8, true);
                string folderName = "Apps\\fmr\\Media\\" + foldpath;
                string dr = Directory.GetCurrentDirectory();
                string newPath = Path.Combine(Directory.GetParent(dr).Parent.ToString(), folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                string urlpath = "https://fmr.logistic-solutions.com/media/" + foldpath + "/";
                string fileName = "";
                if (file.Length > 0)
                {
                    fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    Answer.Videofile = foldpath + "/" + fileName;
                    _context.AssesmentCandidate.Add(Answer);
                    await _context.SaveChangesAsync();
                    return CreatedAtAction("ApplyJob", new { id = Answer.Id }, Answer);
                }
                else
                {
                    return Ok(new { StatusCode = StatusCodes.Status200OK, message = "No Video File Uploaded.", result = urlpath + fileName });
                }
            }
            catch (System.Exception ex)
            {
                return Ok(new { StatusCode = StatusCodes.Status417ExpectationFailed, message = ex.Message });
            }
        }


    }

}
