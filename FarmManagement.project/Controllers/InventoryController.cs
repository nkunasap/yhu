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
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("sub") ?? "0");

        // GET api/inventory
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.InventoryItems
                .Where(i => i.UserId == CurrentUserId)
                .Select(i => new {
                    i.Id, i.Name, i.Category, i.Quantity, i.Unit, i.LastUpdated, i.UserId
                })
                .ToListAsync();
            return Ok(items);
        }

        // GET api/inventory/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.InventoryItems
                .Where(i => i.Id == id && i.UserId == CurrentUserId)
                .Select(i => new {
                    i.Id, i.Name, i.Category, i.Quantity, i.Unit, i.LastUpdated, i.UserId
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST api/inventory
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InventoryDto dto)
        {
            var item = new InventoryItem
            {
                Name        = dto.Name,
                Category    = dto.Category,
                Quantity    = dto.Quantity,
                Unit        = dto.Unit,
                LastUpdated = DateTime.UtcNow,
                UserId      = CurrentUserId
            };
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.Id },
                new { item.Id, item.Name, item.Category, item.Quantity, item.Unit, item.LastUpdated, item.UserId });
        }

        // PUT api/inventory/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InventoryDto dto)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);

            if (item == null) return NotFound();

            item.Name        = dto.Name;
            item.Category    = dto.Category;
            item.Quantity    = dto.Quantity;
            item.Unit        = dto.Unit;
            item.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/inventory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);

            if (item == null) return NotFound();

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class InventoryDto
    {
        public string Name     { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int    Quantity { get; set; }
        public string Unit     { get; set; } = string.Empty;
    }
}
