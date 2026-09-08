namespace FarmManagement.API.Models
{
    public class GrowingZone
    {
        public int      Id                  { get; set; }
        public string   Name                { get; set; } = string.Empty; // e.g. "Zone 01"

        // WF-005: Crop assignment
        public string   CurrentCrop         { get; set; } = string.Empty;
        public DateTime? PlantingDate       { get; set; }
        public string   GrowthStage         { get; set; } = string.Empty; // Seed / Seedling / Vegetative / …

        // Area / capacity
        public decimal  AreaM2              { get; set; }    // zone area in m²
        public int      PlantCapacity       { get; set; }    // max number of plants
        public decimal  RowSpacingCm        { get; set; }    // supports density calculations
        public decimal  PlantSpacingCm      { get; set; }

        // AI operating targets — environmental
        public decimal  TargetTempMinC      { get; set; }
        public decimal  TargetTempMaxC      { get; set; }
        public decimal  TargetHumidityMinPct{ get; set; }
        public decimal  TargetHumidityMaxPct{ get; set; }
        public decimal  TargetCO2Ppm        { get; set; } = 800;
        public decimal  TargetLightLux      { get; set; }
        public decimal  TargetPhMin         { get; set; }
        public decimal  TargetPhMax         { get; set; }
        public decimal  TargetEcMin         { get; set; }   // mS/cm
        public decimal  TargetEcMax         { get; set; }

        // Expected harvest
        public DateTime? ExpectedHarvestDate { get; set; }

        public string   Notes               { get; set; } = string.Empty;
        public DateTime CreatedAt           { get; set; } = DateTime.UtcNow;

        // FK → Greenhouse
        public int      GreenhouseId        { get; set; }
        public Greenhouse Greenhouse        { get; set; } = null!;
    }
}
