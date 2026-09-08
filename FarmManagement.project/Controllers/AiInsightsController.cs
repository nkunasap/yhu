using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FarmManagement.API.Data;
using FarmManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API.Controllers
{
    /// <summary>
    /// Exposes the FarmSense AI engine to the dashboard. Assembles a snapshot
    /// of the current user's farm data + live weather, then hands it to the
    /// engine to produce prioritised, explainable insights.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiInsightsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWeatherService _weather;
        private readonly IAiInsightsEngine _engine;

        private const double DefaultLat = -25.7479;
        private const double DefaultLon = 28.2293;
        private const string DefaultLabel = "Pretoria, South Africa";

        public AiInsightsController(ApplicationDbContext context, IWeatherService weather, IAiInsightsEngine engine)
        {
            _context = context;
            _weather = weather;
            _engine = engine;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "0");

        // GET api/aiinsights?farmId=5
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? farmId)
        {
            var farmQuery = _context.Farms.Where(f => f.UserId == CurrentUserId);
            var farm = farmId.HasValue
                ? await farmQuery.FirstOrDefaultAsync(f => f.Id == farmId.Value)
                : await farmQuery.OrderBy(f => f.Id).FirstOrDefaultAsync();

            var snapshot = new FarmSnapshot
            {
                FarmId = farm?.Id ?? 0,
                FarmName = farm?.Name ?? "Your farm"
            };

            // Zones (only for greenhouses belonging to this farm, if we have one)
            if (farm != null)
            {
                snapshot.Zones = await _context.GrowingZones
                    .Where(z => z.Greenhouse.FarmId == farm.Id)
                    .Select(z => new ZoneSnapshot
                    {
                        Name = z.Name,
                        GreenhouseName = z.Greenhouse.Name,
                        HasClimateControl = z.Greenhouse.HasClimateControl,
                        CurrentCrop = z.CurrentCrop,
                        GrowthStage = z.GrowthStage,
                        TargetTempMinC = z.TargetTempMinC,
                        TargetTempMaxC = z.TargetTempMaxC,
                        TargetHumidityMinPct = z.TargetHumidityMinPct,
                        TargetHumidityMaxPct = z.TargetHumidityMaxPct,
                        ExpectedHarvestDate = z.ExpectedHarvestDate
                    })
                    .ToListAsync();
            }

            snapshot.Crops = await _context.Crops
                .Where(c => c.UserId == CurrentUserId)
                .Select(c => new CropSnapshot
                {
                    Name = c.Name,
                    FieldLocation = c.FieldLocation,
                    GrowthStage = c.GrowthStage,
                    PlantedDate = c.PlantedDate
                })
                .ToListAsync();

            snapshot.Inventory = await _context.InventoryItems
                .Where(i => i.UserId == CurrentUserId)
                .Select(i => new InventorySnapshot
                {
                    Name = i.Name,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                })
                .ToListAsync();

            snapshot.Tasks = await _context.FarmTasks
                .Where(t => t.UserId == CurrentUserId)
                .Select(t => new TaskSnapshot
                {
                    Title = t.Title,
                    Status = t.Status,
                    DueDate = t.DueDate
                })
                .ToListAsync();

            // Live weather feeds several of the engine's rules
            double lat = farm?.Latitude ?? DefaultLat;
            double lon = farm?.Longitude ?? DefaultLon;
            string label = farm != null && !string.IsNullOrWhiteSpace(farm.Address) ? farm.Address : DefaultLabel;

            try
            {
                snapshot.Weather = await _weather.GetLiveWeatherAsync(lat, lon, label);
            }
            catch
            {
                snapshot.Weather = null; // engine still runs fine on farm-data-only rules
            }

            var response = _engine.BuildResponse(snapshot);
            return Ok(response);
        }
    }
}
