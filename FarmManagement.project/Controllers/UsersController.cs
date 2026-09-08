using FarmManagement.API.Data;
using FarmManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id, u.FullName, u.Email, u.Role,
                    u.Department, u.IsActive, u.CreatedAt,
                    taskCount = _context.FarmTasks.Count(t => t.UserId == u.Id)
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return Ok(users);
        }

        // GET api/users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id, u.FullName, u.Email, u.Role,
                    u.Department, u.IsActive, u.CreatedAt,
                    tasks = _context.FarmTasks
                        .Where(t => t.UserId == u.Id)
                        .Select(t => new { t.Id, t.Title, t.Status, t.DueDate })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST api/users  — admin creates an employee account
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "A user with this email already exists." });

            var user = new User
            {
                FullName     = dto.FullName,
                Email        = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = dto.Role,
                Department   = dto.Department,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee created.", user.Id, user.FullName, user.Email, user.Role, user.Department });
        }

        // PUT api/users/5/role  — change role & department
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role       = dto.Role;
            user.Department = dto.Department;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role updated.", user.Id, user.FullName, user.Role, user.Department });
        }

        // PUT api/users/5/status  — activate / deactivate
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = dto.IsActive ? "User activated." : "User deactivated.", user.Id, user.IsActive });
        }

        // POST api/users/5/tasks  — assign a task to an employee
        [HttpPost("{id}/tasks")]
        public async Task<IActionResult> AssignTask(int id, [FromBody] AssignTaskDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var task = new FarmTask
            {
                Title       = dto.Title,
                Description = dto.Description,
                Status      = "Pending",
                DueDate     = dto.DueDate,
                CreatedAt   = DateTime.UtcNow,
                UserId      = id
            };

            _context.FarmTasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task assigned.", task.Id, task.Title, task.DueDate, assignedTo = user.FullName });
        }

        // DELETE api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee removed." });
        }
    }

    // ── DTOs ─────────────────────────────────────────────
    public class CreateEmployeeDto
    {
        public string FullName   { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public string Password   { get; set; } = string.Empty;
        public string Role       { get; set; } = "Farmer";
        public string Department { get; set; } = string.Empty;
    }

    public class UpdateRoleDto
    {
        public string Role       { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class UpdateStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class AssignTaskDto
    {
        public string   Title       { get; set; } = string.Empty;
        public string   Description { get; set; } = string.Empty;
        public DateTime DueDate     { get; set; }
    }
}
