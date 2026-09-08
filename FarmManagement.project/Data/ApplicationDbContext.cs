using FarmManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User>          Users          { get; set; }
        public DbSet<Crop>          Crops          { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<FarmTask>      FarmTasks      { get; set; }
        public DbSet<Farm>          Farms          { get; set; }
        public DbSet<Greenhouse>    Greenhouses    { get; set; }
        public DbSet<GrowingZone>   GrowingZones   { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Crop>()
                .HasOne(c => c.User)
                .WithMany(u => u.Crops)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<InventoryItem>()
                .HasOne(i => i.User)
                .WithMany(u => u.InventoryItems)
                .HasForeignKey(i => i.UserId);

            modelBuilder.Entity<FarmTask>()
                .HasOne(t => t.User)
                .WithMany(u => u.FarmTasks)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<Farm>()
                .HasOne(f => f.User)
                .WithMany(u => u.Farms)
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<Farm>()
                .Property(f => f.Area)
                .HasPrecision(18, 4);

            // Greenhouse → Farm
            modelBuilder.Entity<Greenhouse>()
                .HasOne(g => g.Farm)
                .WithMany(f => f.Greenhouses)
                .HasForeignKey(g => g.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            foreach (var prop in new[] { "LengthM","WidthM","HeightM","TargetCO2Ppm" })
                modelBuilder.Entity<Greenhouse>().Property(prop).HasPrecision(18, 4);

            // GrowingZone → Greenhouse
            modelBuilder.Entity<GrowingZone>()
                .HasOne(z => z.Greenhouse)
                .WithMany(g => g.GrowingZones)
                .HasForeignKey(z => z.GreenhouseId)
                .OnDelete(DeleteBehavior.Cascade);

            foreach (var prop in new[] {
                "AreaM2","RowSpacingCm","PlantSpacingCm",
                "TargetTempMinC","TargetTempMaxC",
                "TargetHumidityMinPct","TargetHumidityMaxPct",
                "TargetCO2Ppm","TargetLightLux",
                "TargetPhMin","TargetPhMax",
                "TargetEcMin","TargetEcMax" })
                modelBuilder.Entity<GrowingZone>().Property(prop).HasPrecision(18, 4);
        }
    }
}
