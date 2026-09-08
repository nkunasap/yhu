namespace FarmManagement.API.Models
{
    public class Animal
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Species { get; set; } = string.Empty;

        public string HealthStatus { get; set; } = string.Empty;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
