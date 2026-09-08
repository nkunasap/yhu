namespace FarmManagement.API.Models
{
    public class Crop
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string FieldLocation { get; set; } = string.Empty;

        public string GrowthStage { get; set; } = string.Empty;

        public DateTime PlantedDate { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
