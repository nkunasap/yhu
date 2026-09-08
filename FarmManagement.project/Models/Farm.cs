namespace FarmManagement.API.Models
{
    public class Farm
    {
        public int      Id           { get; set; }

        // WF-003 Step 1 fields
        public string   Name         { get; set; } = string.Empty;   // farms.name
        public string   FarmType     { get; set; } = string.Empty;   // farms.type
        public string   Address      { get; set; } = string.Empty;   // farms.address
        public double?  Latitude     { get; set; }                   // farms.latitude
        public double?  Longitude    { get; set; }                   // farms.longitude
        public decimal  Area         { get; set; }                   // farms.area
        public string   AreaUnit     { get; set; } = "m²";           // unit for area display
        public string   PrimaryCrop  { get; set; } = string.Empty;   // initial AI context

        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;

        // FK → User
        public int      UserId       { get; set; }
        public User     User         { get; set; } = null!;

        // Children
        public ICollection<Greenhouse> Greenhouses { get; set; } = new List<Greenhouse>();
    }
}
