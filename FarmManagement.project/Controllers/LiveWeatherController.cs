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
    /// Live weather for the dashboard's left-hand widget.
    /// Falls back to the user's first registered farm location, then to a
    /// sensible default, so the widget always has something to show.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LiveWeatherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWeatherService _weather;

        // Default fallback location (Pretoria, South Africa) used only when
        // the user has no farm with coordinates yet and none were supplied.
        private const double DefaultLat = -25.7479;
        private const double DefaultLon = 28.2293;
        private const string DefaultLabel = "Pretoria, South Africa";

        public LiveWeatherController(ApplicationDbContext context, IWeatherService weather)
        {
            _context = context;
            _weather = weather;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "0");

        // GET api/liveweather
        // GET api/liveweather?lat=-25.7&lon=28.2&label=My+Farm
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] double? lat, [FromQuery] double? lon, [FromQuery] string? label)
        {
            double latitude, longitude;
            string locationLabel;

            if (lat.HasValue && lon.HasValue)
            {
                latitude = lat.Value;
                longitude = lon.Value;
                locationLabel = string.IsNullOrWhiteSpace(label) ? "Your location" : label!;
            }
            else
            {
                var farm = await _context.Farms
                    .Where(f => f.UserId == CurrentUserId && f.Latitude != null && f.Longitude != null)
                    .OrderBy(f => f.Id)
                    .FirstOrDefaultAsync();

                if (farm != null)
                {
                    latitude = farm.Latitude!.Value;
                    longitude = farm.Longitude!.Value;
                    locationLabel = string.IsNullOrWhiteSpace(farm.Address) ? farm.Name : farm.Address;
                }
                else
                {
                    latitude = DefaultLat;
                    longitude = DefaultLon;
                    locationLabel = DefaultLabel;
                }
            }

            try
            {
                var result = await _weather.GetLiveWeatherAsync(latitude, longitude, locationLabel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = "Live weather is temporarily unavailable.", detail = ex.Message });
            }
        }
    }
}
