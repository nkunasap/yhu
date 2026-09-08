namespace FarmManagement.API.Services
{
    public class AiInsight
    {
        public string Id          { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Category    { get; set; } = "general"; // weather | irrigation | climate | pest | inventory | tasks | growth | general
        public string Severity    { get; set; } = "info";    // info | advisory | warning | critical
        public string Icon        { get; set; } = "🤖";
        public string Title       { get; set; } = string.Empty;
        public string Message     { get; set; } = string.Empty;
        public int    Confidence  { get; set; } = 80;         // 0-100, model's self-reported confidence
    }

    public class AiInsightsResponse
    {
        public string EngineName    { get; set; } = "FarmSense AI";
        public string EngineVersion { get; set; } = "1.0";
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int?    FarmId       { get; set; }
        public string  FarmName     { get; set; } = string.Empty;
        public string  Headline     { get; set; } = string.Empty;
        public int     OverallScore { get; set; } = 100; // 0-100 farm "health" style score
        public List<AiInsight> Insights { get; set; } = new();
    }

    /// <summary>
    /// Everything the AI engine is allowed to look at for one run.
    /// Kept as a plain snapshot so the engine itself has no DB/HTTP dependency
    /// and is trivially unit-testable / explainable.
    /// </summary>
    public class FarmSnapshot
    {
        public int    FarmId   { get; set; }
        public string FarmName { get; set; } = "Your farm";

        public LiveWeatherResult? Weather { get; set; }

        public List<ZoneSnapshot>      Zones      { get; set; } = new();
        public List<CropSnapshot>      Crops      { get; set; } = new();
        public List<InventorySnapshot> Inventory  { get; set; } = new();
        public List<TaskSnapshot>      Tasks      { get; set; } = new();
    }

    public class ZoneSnapshot
    {
        public string Name                  { get; set; } = string.Empty;
        public string GreenhouseName        { get; set; } = string.Empty;
        public bool   HasClimateControl     { get; set; }
        public string CurrentCrop           { get; set; } = string.Empty;
        public string GrowthStage           { get; set; } = string.Empty;
        public decimal TargetTempMinC       { get; set; }
        public decimal TargetTempMaxC       { get; set; }
        public decimal TargetHumidityMinPct { get; set; }
        public decimal TargetHumidityMaxPct { get; set; }
        public DateTime? ExpectedHarvestDate{ get; set; }
    }

    public class CropSnapshot
    {
        public string Name         { get; set; } = string.Empty;
        public string FieldLocation{ get; set; } = string.Empty;
        public string GrowthStage  { get; set; } = string.Empty;
        public DateTime PlantedDate{ get; set; }
    }

    public class InventorySnapshot
    {
        public string Name     { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int    Quantity { get; set; }
        public string Unit     { get; set; } = string.Empty;
    }

    public class TaskSnapshot
    {
        public string Title    { get; set; } = string.Empty;
        public string Status   { get; set; } = string.Empty;
        public DateTime DueDate{ get; set; }
    }
}
