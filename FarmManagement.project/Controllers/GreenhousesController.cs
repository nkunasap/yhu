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
    public class GreenhousesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GreenhousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "0");

        // Verify the farm belongs to the current user
        private async Task<bool> FarmBelongsToUser(int farmId) =>
            await _context.Farms.AnyAsync(f => f.Id == farmId && f.UserId == CurrentUserId);

        // GET api/greenhouses?farmId=1
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? farmId)
        {
            var query = _context.Greenhouses
                .Where(g => g.Farm.UserId == CurrentUserId);

            if (farmId.HasValue)
                query = query.Where(g => g.FarmId == farmId.Value);

            var list = await query
                .Select(g => new {
                    g.Id, g.Name, g.FarmId,
                    g.LengthM, g.WidthM, g.HeightM,
                    g.HasClimateControl, g.HasHeating, g.HasCooling, g.HasHumidityControl,
                    g.IrrigationSystem, g.HasAutomatedIrrig,
                    g.LightingSystem, g.HasSupplementalLight,
                    g.HasCO2Injection, g.TargetCO2Ppm,
                    g.Notes, g.CreatedAt,
                    zoneCount = g.GrowingZones.Count
                })
                .OrderBy(g => g.Name)
                .ToListAsync();

            return Ok(list);
        }

        // GET api/greenhouses/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var g = await _context.Greenhouses
                .Where(g => g.Id == id && g.Farm.UserId == CurrentUserId)
                .Select(g => new {
                    g.Id, g.Name, g.FarmId,
                    g.LengthM, g.WidthM, g.HeightM,
                    g.HasClimateControl, g.HasHeating, g.HasCooling, g.HasHumidityControl,
                    g.IrrigationSystem, g.HasAutomatedIrrig,
                    g.LightingSystem, g.HasSupplementalLight,
                    g.HasCO2Injection, g.TargetCO2Ppm,
                    g.Notes, g.CreatedAt,
                    zones = g.GrowingZones.Select(z => new {
                        z.Id, z.Name, z.CurrentCrop, z.GrowthStage, z.AreaM2
                    })
                })
                .FirstOrDefaultAsync();

            if (g == null) return NotFound();
            return Ok(g);
        }

        // POST api/greenhouses
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GreenhouseDto dto)
        {
            if (!await FarmBelongsToUser(dto.FarmId))
                return BadRequest(new { message = "Farm not found or does not belong to you." });

            var gh = new Greenhouse
            {
                Name                = dto.Name,
                FarmId              = dto.FarmId,
                LengthM             = dto.LengthM,
                WidthM              = dto.WidthM,
                HeightM             = dto.HeightM,
                HasClimateControl   = dto.HasClimateControl,
                HasHeating          = dto.HasHeating,
                HasCooling          = dto.HasCooling,
                HasHumidityControl  = dto.HasHumidityControl,
                IrrigationSystem    = dto.IrrigationSystem,
                HasAutomatedIrrig   = dto.HasAutomatedIrrig,
                LightingSystem      = dto.LightingSystem,
                HasSupplementalLight= dto.HasSupplementalLight,
                HasCO2Injection     = dto.HasCO2Injection,
                TargetCO2Ppm        = dto.TargetCO2Ppm,
                Notes               = dto.Notes,
                CreatedAt           = DateTime.UtcNow
            };

            _context.Greenhouses.Add(gh);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = gh.Id }, new {
                gh.Id, gh.Name, gh.FarmId,
                gh.LengthM, gh.WidthM, gh.HeightM,
                gh.HasClimateControl, gh.HasHeating, gh.HasCooling, gh.HasHumidityControl,
                gh.IrrigationSystem, gh.HasAutomatedIrrig,
                gh.LightingSystem, gh.HasSupplementalLight,
                gh.HasCO2Injection, gh.TargetCO2Ppm,
                gh.Notes, gh.CreatedAt
            });
        }

        // PUT api/greenhouses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GreenhouseDto dto)
        {
            var gh = await _context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == id && g.Farm.UserId == CurrentUserId);

            if (gh == null) return NotFound();

            gh.Name                = dto.Name;
            gh.LengthM             = dto.LengthM;
            gh.WidthM              = dto.WidthM;
            gh.HeightM             = dto.HeightM;
            gh.HasClimateControl   = dto.HasClimateControl;
            gh.HasHeating          = dto.HasHeating;
            gh.HasCooling          = dto.HasCooling;
            gh.HasHumidityControl  = dto.HasHumidityControl;
            gh.IrrigationSystem    = dto.IrrigationSystem;
            gh.HasAutomatedIrrig   = dto.HasAutomatedIrrig;
            gh.LightingSystem      = dto.LightingSystem;
            gh.HasSupplementalLight= dto.HasSupplementalLight;
            gh.HasCO2Injection     = dto.HasCO2Injection;
            gh.TargetCO2Ppm        = dto.TargetCO2Ppm;
            gh.Notes               = dto.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/greenhouses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var gh = await _context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == id && g.Farm.UserId == CurrentUserId);

            if (gh == null) return NotFound();

            _context.Greenhouses.Remove(gh);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class GreenhouseDto
    {
        public string  Name                { get; set; } = string.Empty;
        public int     FarmId              { get; set; }
        public decimal LengthM             { get; set; }
        public decimal WidthM              { get; set; }
        public decimal HeightM             { get; set; }
        public bool    HasClimateControl   { get; set; }
        public bool    HasHeating          { get; set; }
        public bool    HasCooling          { get; set; }
        public bool    HasHumidityControl  { get; set; }
        public string  IrrigationSystem    { get; set; } = string.Empty;
        public bool    HasAutomatedIrrig   { get; set; }
        public string  LightingSystem      { get; set; } = string.Empty;
        public bool    HasSupplementalLight{ get; set; }
        public bool    HasCO2Injection     { get; set; }
        public decimal TargetCO2Ppm        { get; set; } = 400;
        public string  Notes               { get; set; } = string.Empty;
    }
}
