using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.cabcheap.com.Data;
using api.cabcheap.com.Models;

namespace api.cabcheap.com.Controllers.api
{
    [Produces("application/json")]
    [Route("api/Waypoints")]
    public class WaypointsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WaypointsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Waypoints
        [HttpGet]
        public IEnumerable<Waypoint> GetWaypoints()
        {
            return _context.Waypoints;
        }

        // GET: api/Waypoints/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWaypoint([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var waypoint = await _context.Waypoints.SingleOrDefaultAsync(m => m.Id == id);

            if (waypoint == null)
            {
                return NotFound();
            }

            return Ok(waypoint);
        }

        // PUT: api/Waypoints/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWaypoint([FromRoute] int id, [FromBody] Waypoint waypoint)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != waypoint.Id)
            {
                return BadRequest();
            }

            _context.Entry(waypoint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WaypointExists(id))
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

        // POST: api/Waypoints
        [HttpPost]
        public async Task<IActionResult> PostWaypoint([FromBody] Waypoint waypoint)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Waypoints.Add(waypoint);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWaypoint", new { id = waypoint.Id }, waypoint);
        }

        // DELETE: api/Waypoints/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWaypoint([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var waypoint = await _context.Waypoints.SingleOrDefaultAsync(m => m.Id == id);
            if (waypoint == null)
            {
                return NotFound();
            }

            _context.Waypoints.Remove(waypoint);
            await _context.SaveChangesAsync();

            return Ok(waypoint);
        }

        private bool WaypointExists(int id)
        {
            return _context.Waypoints.Any(e => e.Id == id);
        }
    }
}