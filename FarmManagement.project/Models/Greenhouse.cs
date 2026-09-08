namespace FarmManagement.API.Models
{
    public class Greenhouse
    {
        public int     Id                  { get; set; }
        public string  Name                { get; set; } = string.Empty; // e.g. "Greenhouse 01"

        // Dimensions
        public decimal LengthM             { get; set; }  // metres
        public decimal WidthM              { get; set; }
        public decimal HeightM             { get; set; }

        // Climate-control capabilities
        public bool    HasClimateControl   { get; set; } = false;
        public bool    HasHeating          { get; set; } = false;
        public bool    HasCooling          { get; set; } = false;
        public bool    HasHumidityControl  { get; set; } = false;

        // Irrigation system
        public string  IrrigationSystem    { get; set; } = string.Empty; // Drip / NFT / Flood / None
        public bool    HasAutomatedIrrig   { get; set; } = false;

        // Lighting system
        public string  LightingSystem      { get; set; } = string.Empty; // LED / HPS / Natural / None
        public bool    HasSupplementalLight{ get; set; } = false;

        // CO₂ capability
        public bool    HasCO2Injection     { get; set; } = false;
        public decimal TargetCO2Ppm        { get; set; } = 400;

        public string  Notes               { get; set; } = string.Empty;
        public DateTime CreatedAt          { get; set; } = DateTime.UtcNow;

        // FK → Farm
        public int     FarmId              { get; set; }
        public Farm    Farm                { get; set; } = null!;

        // Children
        public ICollection<GrowingZone> GrowingZones { get; set; } = new List<GrowingZone>();
    }
}
