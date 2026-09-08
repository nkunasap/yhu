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
    public class CropsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CropsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue("sub") ?? "0");

        // GET api/crops
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var crops = await _context.Crops
                .Where(c => c.UserId == CurrentUserId)
                .Select(c => new {
                    c.Id, c.Name, c.FieldLocation, c.GrowthStage, c.PlantedDate, c.UserId
                })
                .ToListAsync();
            return Ok(crops);
        }

        // GET api/crops/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var crop = await _context.Crops
                .Where(c => c.Id == id && c.UserId == CurrentUserId)
                .Select(c => new {
                    c.Id, c.Name, c.FieldLocation, c.GrowthStage, c.PlantedDate, c.UserId
                })
                .FirstOrDefaultAsync();

            if (crop == null) return NotFound();
            return Ok(crop);
        }

        // POST api/crops
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CropDto dto)
        {
            var crop = new Crop
            {
                Name          = dto.Name,
                FieldLocation = dto.FieldLocation,
                GrowthStage   = dto.GrowthStage,
                PlantedDate   = dto.PlantedDate == default ? DateTime.UtcNow : dto.PlantedDate,
                UserId        = CurrentUserId
            };
            _context.Crops.Add(crop);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = crop.Id },
                new { crop.Id, crop.Name, crop.FieldLocation, crop.GrowthStage, crop.PlantedDate, crop.UserId });
        }

        // PUT api/crops/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CropDto dto)
        {
            var crop = await _context.Crops
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

            if (crop == null) return NotFound();

            crop.Name          = dto.Name;
            crop.FieldLocation = dto.FieldLocation;
            crop.GrowthStage   = dto.GrowthStage;
            if (dto.PlantedDate != default) crop.PlantedDate = dto.PlantedDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/crops/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var crop = await _context.Crops
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

            if (crop == null) return NotFound();

            _context.Crops.Remove(crop);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CropDto
    {
        public string   Name          { get; set; } = string.Empty;
        public string   FieldLocation { get; set; } = string.Empty;
        public string   GrowthStage   { get; set; } = string.Empty;
        public DateTime PlantedDate   { get; set; }
    }
}
