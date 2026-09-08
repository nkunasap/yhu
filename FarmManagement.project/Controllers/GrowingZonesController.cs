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
    public class GrowingZonesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GrowingZonesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "0");

        // Verify the greenhouse belongs to the current user (via its farm)
        private async Task<bool> GreenhouseBelongsToUser(int greenhouseId) =>
            await _context.Greenhouses.AnyAsync(g => g.Id == greenhouseId && g.Farm.UserId == CurrentUserId);

        // GET api/growingzones?greenhouseId=1
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? greenhouseId)
        {
            var query = _context.GrowingZones
                .Where(z => z.Greenhouse.Farm.UserId == CurrentUserId);

            if (greenhouseId.HasValue)
                query = query.Where(z => z.GreenhouseId == greenhouseId.Value);

            var list = await query
                .Select(z => new {
                    z.Id, z.Name, z.GreenhouseId,
                    z.CurrentCrop, z.PlantingDate, z.GrowthStage,
                    z.AreaM2, z.PlantCapacity,
                    z.RowSpacingCm, z.PlantSpacingCm,
                    z.TargetTempMinC, z.TargetTempMaxC,
                    z.TargetHumidityMinPct, z.TargetHumidityMaxPct,
                    z.TargetCO2Ppm, z.TargetLightLux,
                    z.TargetPhMin, z.TargetPhMax,
                    z.TargetEcMin, z.TargetEcMax,
                    z.ExpectedHarvestDate, z.Notes, z.CreatedAt,
                    greenhouseName = z.Greenhouse.Name
                })
                .OrderBy(z => z.Name)
                .ToListAsync();

            return Ok(list);
        }

        // GET api/growingzones/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var z = await _context.GrowingZones
                .Where(z => z.Id == id && z.Greenhouse.Farm.UserId == CurrentUserId)
                .Select(z => new {
                    z.Id, z.Name, z.GreenhouseId,
                    z.CurrentCrop, z.PlantingDate, z.GrowthStage,
                    z.AreaM2, z.PlantCapacity,
                    z.RowSpacingCm, z.PlantSpacingCm,
                    z.TargetTempMinC, z.TargetTempMaxC,
                    z.TargetHumidityMinPct, z.TargetHumidityMaxPct,
                    z.TargetCO2Ppm, z.TargetLightLux,
                    z.TargetPhMin, z.TargetPhMax,
                    z.TargetEcMin, z.TargetEcMax,
                    z.ExpectedHarvestDate, z.Notes, z.CreatedAt,
                    greenhouseName = z.Greenhouse.Name
                })
                .FirstOrDefaultAsync();

            if (z == null) return NotFound();
            return Ok(z);
        }

        // POST api/growingzones
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GrowingZoneDto dto)
        {
            if (!await GreenhouseBelongsToUser(dto.GreenhouseId))
                return BadRequest(new { message = "Greenhouse not found or does not belong to you." });

            var zone = new GrowingZone
            {
                Name                 = dto.Name,
                GreenhouseId         = dto.GreenhouseId,
                CurrentCrop          = dto.CurrentCrop,
                PlantingDate         = dto.PlantingDate,
                GrowthStage          = dto.GrowthStage,
                AreaM2               = dto.AreaM2,
                PlantCapacity        = dto.PlantCapacity,
                RowSpacingCm         = dto.RowSpacingCm,
                PlantSpacingCm       = dto.PlantSpacingCm,
                TargetTempMinC       = dto.TargetTempMinC,
                TargetTempMaxC       = dto.TargetTempMaxC,
                TargetHumidityMinPct = dto.TargetHumidityMinPct,
                TargetHumidityMaxPct = dto.TargetHumidityMaxPct,
                TargetCO2Ppm         = dto.TargetCO2Ppm,
                TargetLightLux       = dto.TargetLightLux,
                TargetPhMin          = dto.TargetPhMin,
                TargetPhMax          = dto.TargetPhMax,
                TargetEcMin          = dto.TargetEcMin,
                TargetEcMax          = dto.TargetEcMax,
                ExpectedHarvestDate  = dto.ExpectedHarvestDate,
                Notes                = dto.Notes,
                CreatedAt            = DateTime.UtcNow
            };

            _context.GrowingZones.Add(zone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = zone.Id },
                new { zone.Id, zone.Name, zone.GreenhouseId, zone.CurrentCrop, zone.GrowthStage, zone.AreaM2 });
        }

        // PUT api/growingzones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GrowingZoneDto dto)
        {
            var zone = await _context.GrowingZones
                .FirstOrDefaultAsync(z => z.Id == id && z.Greenhouse.Farm.UserId == CurrentUserId);

            if (zone == null) return NotFound();

            zone.Name                 = dto.Name;
            zone.CurrentCrop          = dto.CurrentCrop;
            zone.PlantingDate         = dto.PlantingDate;
            zone.GrowthStage          = dto.GrowthStage;
            zone.AreaM2               = dto.AreaM2;
            zone.PlantCapacity        = dto.PlantCapacity;
            zone.RowSpacingCm         = dto.RowSpacingCm;
            zone.PlantSpacingCm       = dto.PlantSpacingCm;
            zone.TargetTempMinC       = dto.TargetTempMinC;
            zone.TargetTempMaxC       = dto.TargetTempMaxC;
            zone.TargetHumidityMinPct = dto.TargetHumidityMinPct;
            zone.TargetHumidityMaxPct = dto.TargetHumidityMaxPct;
            zone.TargetCO2Ppm         = dto.TargetCO2Ppm;
            zone.TargetLightLux       = dto.TargetLightLux;
            zone.TargetPhMin          = dto.TargetPhMin;
            zone.TargetPhMax          = dto.TargetPhMax;
            zone.TargetEcMin          = dto.TargetEcMin;
            zone.TargetEcMax          = dto.TargetEcMax;
            zone.ExpectedHarvestDate  = dto.ExpectedHarvestDate;
            zone.Notes                = dto.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/growingzones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var zone = await _context.GrowingZones
                .FirstOrDefaultAsync(z => z.Id == id && z.Greenhouse.Farm.UserId == CurrentUserId);

            if (zone == null) return NotFound();

            _context.GrowingZones.Remove(zone);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class GrowingZoneDto
    {
        public string    Name                 { get; set; } = string.Empty;
        public int       GreenhouseId         { get; set; }
        public string    CurrentCrop          { get; set; } = string.Empty;
        public DateTime? PlantingDate         { get; set; }
        public string    GrowthStage          { get; set; } = string.Empty;
        public decimal   AreaM2               { get; set; }
        public int       PlantCapacity        { get; set; }
        public decimal   RowSpacingCm         { get; set; }
        public decimal   PlantSpacingCm       { get; set; }
        public decimal   TargetTempMinC       { get; set; }
        public decimal   TargetTempMaxC       { get; set; }
        public decimal   TargetHumidityMinPct { get; set; }
        public decimal   TargetHumidityMaxPct { get; set; }
        public decimal   TargetCO2Ppm         { get; set; } = 800;
        public decimal   TargetLightLux       { get; set; }
        public decimal   TargetPhMin          { get; set; }
        public decimal   TargetPhMax          { get; set; }
        public decimal   TargetEcMin          { get; set; }
        public decimal   TargetEcMax          { get; set; }
        public DateTime? ExpectedHarvestDate  { get; set; }
        public string    Notes                { get; set; } = string.Empty;
    }
}
