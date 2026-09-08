using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FarmsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FarmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "0");

        // GET api/farms
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var farms = await _context.Farms
                .Where(f => f.UserId == CurrentUserId)
                .Select(f => new {
                    f.Id, f.Name, f.FarmType, f.Address,
                    f.Latitude, f.Longitude,
                    f.Area, f.AreaUnit, f.PrimaryCrop,
                    f.CreatedAt, f.UserId
                })
                .OrderBy(f => f.Name)
                .ToListAsync();

            return Ok(farms);
        }

        // GET api/farms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var farm = await _context.Farms
                .Where(f => f.Id == id && f.UserId == CurrentUserId)
                .Select(f => new {
                    f.Id, f.Name, f.FarmType, f.Address,
                    f.Latitude, f.Longitude,
                    f.Area, f.AreaUnit, f.PrimaryCrop,
                    f.CreatedAt, f.UserId
                })
                .FirstOrDefaultAsync();

            if (farm == null) return NotFound();
            return Ok(farm);
        }

        // POST api/farms
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FarmDto dto)
        {
            var farm = new Farm
            {
                Name        = dto.Name,
                FarmType    = dto.FarmType,
                Address     = dto.Address,
                Latitude    = dto.Latitude,
                Longitude   = dto.Longitude,
                Area        = dto.Area,
                AreaUnit    = dto.AreaUnit,
                PrimaryCrop = dto.PrimaryCrop,
                CreatedAt   = DateTime.UtcNow,
                UserId      = CurrentUserId
            };

            _context.Farms.Add(farm);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = farm.Id }, new {
                farm.Id, farm.Name, farm.FarmType, farm.Address,
                farm.Latitude, farm.Longitude,
                farm.Area, farm.AreaUnit, farm.PrimaryCrop,
                farm.CreatedAt, farm.UserId
            });
        }

        // PUT api/farms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FarmDto dto)
        {
            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == CurrentUserId);

            if (farm == null) return NotFound();

            farm.Name        = dto.Name;
            farm.FarmType    = dto.FarmType;
            farm.Address     = dto.Address;
            farm.Latitude    = dto.Latitude;
            farm.Longitude   = dto.Longitude;
            farm.Area        = dto.Area;
            farm.AreaUnit    = dto.AreaUnit;
            farm.PrimaryCrop = dto.PrimaryCrop;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/farms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == CurrentUserId);

            if (farm == null) return NotFound();

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class FarmDto
    {
        public string   Name        { get; set; } = string.Empty;
        public string   FarmType    { get; set; } = string.Empty;
        public string   Address     { get; set; } = string.Empty;
        public double?  Latitude    { get; set; }
        public double?  Longitude   { get; set; }
        public decimal  Area        { get; set; }
        public string   AreaUnit    { get; set; } = "m²";
        public string   PrimaryCrop { get; set; } = string.Empty;
    }
}
