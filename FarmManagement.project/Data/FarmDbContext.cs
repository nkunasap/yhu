using FarmManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmManagement.API.Data
{
    public class FarmDbContext : DbContext
    {
        public FarmDbContext(DbContextOptions<FarmDbContext> options) : base(options)
        {
        }

        public DbSet<User>          Users          { get; set; }
        public DbSet<Crop>          Crops          { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<FarmTask>      FarmTasks      { get; set; }

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
        }
    }
}
