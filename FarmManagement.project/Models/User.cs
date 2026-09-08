namespace FarmManagement.API.Models
{
    public class User
    {
        public int      Id           { get; set; }
        public string   FullName     { get; set; } = string.Empty;
        public string   Email        { get; set; } = string.Empty;
        public string   PasswordHash { get; set; } = string.Empty;
        public string   Role         { get; set; } = "Farmer";
        public string   Department   { get; set; } = string.Empty;
        public bool     IsActive     { get; set; } = true;
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;

        public ICollection<Crop>          Crops          { get; set; } = new List<Crop>();
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<FarmTask>      FarmTasks      { get; set; } = new List<FarmTask>();
        public ICollection<Farm>          Farms          { get; set; } = new List<Farm>();
    }
}
