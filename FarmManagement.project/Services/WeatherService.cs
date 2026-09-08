using System.Globalization;
using System.Text.Json;

namespace FarmManagement.API.Services
{
    /// <summary>
    /// Fetches live weather + forecast data from Open-Meteo (https://open-meteo.com).
    /// No API key is required, which keeps the farm dashboard working out of the box.
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _http;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(HttpClient http, ILogger<WeatherService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<LiveWeatherResult> GetLiveWeatherAsync(double latitude, double longitude, string? locationLabel = null)
        {
            var url =
                "https://api.open-meteo.com/v1/forecast" +
                $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                "&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation," +
                "weather_code,wind_speed_10m,wind_gusts_10m,is_day,uv_index" +
                "&hourly=temperature_2m,precipitation_probability,weather_code" +
                "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,precipitation_sum" +
                "&forecast_days=6&timezone=auto";

            using var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            var current = root.GetProperty("current");
            var code = current.GetProperty("weather_code").GetInt32();
            var (condition, icon) = WeatherCodeMap.Describe(code);

            var result = new LiveWeatherResult
            {
                Latitude        = latitude,
                Longitude       = longitude,
                LocationLabel   = locationLabel ?? $"{latitude:0.###}, {longitude:0.###}",
                TemperatureC    = current.GetProperty("temperature_2m").GetDouble(),
                FeelsLikeC      = current.GetProperty("apparent_temperature").GetDouble(),
                HumidityPct     = current.GetProperty("relative_humidity_2m").GetDouble(),
                WindSpeedKmh    = current.GetProperty("wind_speed_10m").GetDouble(),
                WindGustKmh     = current.TryGetProperty("wind_gusts_10m", out var gust) ? gust.GetDouble() : 0,
                PrecipitationMm = current.GetProperty("precipitation").GetDouble(),
                UvIndex         = current.TryGetProperty("uv_index", out var uv) ? uv.GetDouble() : 0,
                IsDay           = current.GetProperty("is_day").GetInt32() == 1,
                WeatherCode     = code,
                Condition       = condition,
                Icon            = icon,
                Source          = "Open-Meteo (live)"
            };

            // Daily forecast + today's min/max
            if (root.TryGetProperty("daily", out var daily))
            {
                var dates   = daily.GetProperty("time").EnumerateArray().Select(e => e.GetString()!).ToArray();
                var maxes   = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(e => e.GetDouble()).ToArray();
                var mins    = daily.GetProperty("temperature_2m_min").EnumerateArray().Select(e => e.GetDouble()).ToArray();
                var pop     = daily.GetProperty("precipitation_probability_max").EnumerateArray().Select(e => e.GetInt32()).ToArray();
                var psum    = daily.GetProperty("precipitation_sum").EnumerateArray().Select(e => e.GetDouble()).ToArray();
                var codes   = daily.GetProperty("weather_code").EnumerateArray().Select(e => e.GetInt32()).ToArray();

                for (int i = 0; i < dates.Length; i++)
                {
                    var (dCond, dIcon) = WeatherCodeMap.Describe(codes[i]);
                    result.Daily.Add(new DailyForecast
                    {
                        Date = DateOnly.Parse(dates[i]),
                        MaxTempC = maxes[i],
                        MinTempC = mins[i],
                        PrecipitationProbabilityPct = pop.Length > i ? pop[i] : 0,
                        PrecipitationSumMm = psum.Length > i ? psum[i] : 0,
                        WeatherCode = codes[i],
                        Condition = dCond,
                        Icon = dIcon
                    });
                }

                if (result.Daily.Count > 0)
                {
                    result.TodayMaxC = result.Daily[0].MaxTempC;
                    result.TodayMinC = result.Daily[0].MinTempC;
                }
            }

            // Precipitation probability "right now" — take the current hour from hourly data
            if (root.TryGetProperty("hourly", out var hourly))
            {
                var times = hourly.GetProperty("time").EnumerateArray().Select(e => e.GetString()!).ToArray();
                var temps = hourly.GetProperty("temperature_2m").EnumerateArray().Select(e => e.GetDouble()).ToArray();
                var pops  = hourly.GetProperty("precipitation_probability").EnumerateArray().Select(e => e.GetInt32()).ToArray();
                var hcodes = hourly.GetProperty("weather_code").EnumerateArray().Select(e => e.GetInt32()).ToArray();

                var now = DateTime.Now;
                int nearestIdx = 0;
                var bestDelta = double.MaxValue;
                for (int i = 0; i < times.Length; i++)
                {
                    if (DateTime.TryParse(times[i], out var t))
                    {
                        var delta = Math.Abs((t - now).TotalMinutes);
                        if (delta < bestDelta) { bestDelta = delta; nearestIdx = i; }
                    }
                }
                if (pops.Length > nearestIdx)
                    result.PrecipitationProbabilityPct = pops[nearestIdx];

                // next 8 hours from "now"
                for (int i = nearestIdx; i < Math.Min(times.Length, nearestIdx + 8); i++)
                {
                    if (!DateTime.TryParse(times[i], out var t)) continue;
                    result.Hourly.Add(new HourlyForecast
                    {
                        Time = t,
                        TemperatureC = temps.Length > i ? temps[i] : 0,
                        PrecipitationProbabilityPct = pops.Length > i ? pops[i] : 0,
                        WeatherCode = hcodes.Length > i ? hcodes[i] : 0
                    });
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Maps WMO weather codes (used by Open-Meteo) to a human label + emoji icon.
    /// </summary>
    public static class WeatherCodeMap
    {
        public static (string Condition, string Icon) Describe(int code) => code switch
        {
            0 => ("Clear sky", "☀️"),
            1 => ("Mainly clear", "🌤️"),
            2 => ("Partly cloudy", "⛅"),
            3 => ("Overcast", "☁️"),
            45 or 48 => ("Fog", "🌫️"),
            51 or 53 or 55 => ("Drizzle", "🌦️"),
            56 or 57 => ("Freezing drizzle", "🌧️"),
            61 or 63 or 65 => ("Rain", "🌧️"),
            66 or 67 => ("Freezing rain", "🌧️"),
            71 or 73 or 75 => ("Snow", "🌨️"),
            77 => ("Snow grains", "🌨️"),
            80 or 81 or 82 => ("Rain showers", "🌦️"),
            85 or 86 => ("Snow showers", "🌨️"),
            95 => ("Thunderstorm", "⛈️"),
            96 or 99 => ("Thunderstorm w/ hail", "⛈️"),
            _ => ("Unknown", "🌡️")
        };
    }
}
