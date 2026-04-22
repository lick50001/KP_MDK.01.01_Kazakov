using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using SpaceMarket.Api.Models;
using MySqlConnector;

namespace SpaceMarket.Api.Context
{
    public class SpaceMarketContext : DbContext
    {
        private string config = "server=localhost;uid=root;pwd=;database=SpaceMarketDB";

        protected override void OnConfiguring(DbContextOptionsBuilder optBuilder)
        {
            if (!optBuilder.IsConfigured)
                optBuilder.UseMySql(config, ServerVersion.AutoDetect(config));
        }

        public SpaceMarketContext(DbContextOptions<SpaceMarketContext> options) : base(options) { Database.EnsureCreated(); }

        public DbSet<Users> Users { get; set; }
        public DbSet<Items> Items { get; set; }
        public DbSet<Logs> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Items>()
                .HasOne(i => i.User)
                .WithMany(u => u.Items)
                .HasForeignKey(i => i.UserId);

            modelBuilder.Entity<Logs>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserId);
        }
    }
}
