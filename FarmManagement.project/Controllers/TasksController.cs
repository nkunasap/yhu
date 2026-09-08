using FarmManagement.API.Data;
using FarmManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FarmManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("sub") ?? "0");

        // GET api/tasks
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _context.FarmTasks
                .Where(t => t.UserId == CurrentUserId)
                .Select(t => new {
                    t.Id, t.Title, t.Description, t.Status, t.DueDate, t.CreatedAt, t.UserId
                })
                .ToListAsync();
            return Ok(tasks);
        }

        // GET api/tasks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _context.FarmTasks
                .Where(t => t.Id == id && t.UserId == CurrentUserId)
                .Select(t => new {
                    t.Id, t.Title, t.Description, t.Status, t.DueDate, t.CreatedAt, t.UserId
                })
                .FirstOrDefaultAsync();

            if (task == null) return NotFound();
            return Ok(task);
        }

        // POST api/tasks
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskDto dto)
        {
            var task = new FarmTask
            {
                Title       = dto.Title,
                Description = dto.Description,
                Status      = dto.Status ?? "Pending",
                DueDate     = dto.DueDate,
                CreatedAt   = DateTime.UtcNow,
                UserId      = CurrentUserId
            };
            _context.FarmTasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = task.Id },
                new { task.Id, task.Title, task.Description, task.Status, task.DueDate, task.CreatedAt, task.UserId });
        }

        // PUT api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskDto dto)
        {
            var task = await _context.FarmTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (task == null) return NotFound();

            task.Title       = dto.Title;
            task.Description = dto.Description;
            task.Status      = dto.Status ?? task.Status;
            task.DueDate     = dto.DueDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.FarmTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

            if (task == null) return NotFound();

            _context.FarmTasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class TaskDto
    {
        public string   Title       { get; set; } = string.Empty;
        public string   Description { get; set; } = string.Empty;
        public string?  Status      { get; set; }
        public DateTime DueDate     { get; set; }
    }
}
