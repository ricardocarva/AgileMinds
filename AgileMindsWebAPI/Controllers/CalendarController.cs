using AgileMinds.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgileMindsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly DbContext _context;

        public CalendarController(DbContext context)
        {
            _context = context;
        }

        // GET: api/Calendar
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sprint>>> GetSprints()
        {
            return await _context.Set<Sprint>().ToListAsync();
        }

        // GET: api/Calendar/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Sprint>> GetSprint(int id)
        {
            var sprint = await _context.Set<Sprint>().FindAsync(id);

            if (sprint == null)
            {
                return NotFound();
            }

            return sprint;
        }

        // GET: api/Calendar/{id}/start-date
        [HttpGet("{id}/start-date")]
        public async Task<ActionResult<DateTime>> GetSprintStartDate(int id)
        {
            var sprint = await _context.Set<Sprint>().FindAsync(id);

            if (sprint == null)
            {
                return NotFound();
            }

            return sprint.StartDate;
        }

        // GET: api/Calendar/{id}/end-date
        [HttpGet("{id}/end-date")]
        public async Task<ActionResult<DateTime>> GetSprintEndDate(int id)
        {
            var sprint = await _context.Set<Sprint>().FindAsync(id);

            if (sprint == null)
            {
                return NotFound();
            }

            return sprint.EndDate;
        }

        // POST: api/Calendar
        [HttpPost]
        public async Task<ActionResult<Sprint>> CreateSprint(Sprint sprint)
        {
            _context.Set<Sprint>().Add(sprint);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSprint), new { id = sprint.Id }, sprint);
        }

        // PUT: api/Calendar/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSprint(int id, Sprint sprint)
        {
            if (id != sprint.Id)
            {
                return BadRequest();
            }

            _context.Entry(sprint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SprintExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Calendar/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSprint(int id)
        {
            var sprint = await _context.Set<Sprint>().FindAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }

            _context.Set<Sprint>().Remove(sprint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SprintExists(int id)
        {
            return _context.Set<Sprint>().Any(e => e.Id == id);
        }
    }
}