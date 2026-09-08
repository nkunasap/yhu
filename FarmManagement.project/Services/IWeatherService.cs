namespace FarmManagement.API.Services
{
    /// <summary>
    /// Live weather conditions for a single point, normalised for farm use.
    /// </summary>
    public class LiveWeatherResult
    {
        public double  Latitude          { get; set; }
        public double  Longitude         { get; set; }
        public string  LocationLabel     { get; set; } = string.Empty;

        public double  TemperatureC      { get; set; }
        public double  FeelsLikeC        { get; set; }
        public double  HumidityPct       { get; set; }
        public double  WindSpeedKmh      { get; set; }
        public double  WindGustKmh       { get; set; }
        public double  PrecipitationMm   { get; set; }
        public int     PrecipitationProbabilityPct { get; set; }
        public double  UvIndex           { get; set; }
        public bool    IsDay             { get; set; }
        public int     WeatherCode       { get; set; }
        public string  Condition         { get; set; } = string.Empty;
        public string  Icon              { get; set; } = string.Empty;

        public double  TodayMaxC         { get; set; }
        public double  TodayMinC         { get; set; }

        public List<DailyForecast>  Daily  { get; set; } = new();
        public List<HourlyForecast> Hourly { get; set; } = new();

        public DateTime FetchedAtUtc     { get; set; } = DateTime.UtcNow;
        public string  Source            { get; set; } = "Open-Meteo";
    }

    public class DailyForecast
    {
        public DateOnly Date                        { get; set; }
        public double   MaxTempC                    { get; set; }
        public double   MinTempC                     { get; set; }
        public int      PrecipitationProbabilityPct  { get; set; }
        public double   PrecipitationSumMm           { get; set; }
        public int      WeatherCode                  { get; set; }
        public string   Condition                    { get; set; } = string.Empty;
        public string   Icon                         { get; set; } = string.Empty;
    }

    public class HourlyForecast
    {
        public DateTime Time            { get; set; }
        public double   TemperatureC    { get; set; }
        public int      PrecipitationProbabilityPct { get; set; }
        public int      WeatherCode     { get; set; }
    }

    public interface IWeatherService
    {
        Task<LiveWeatherResult> GetLiveWeatherAsync(double latitude, double longitude, string? locationLabel = null);
    }
}
