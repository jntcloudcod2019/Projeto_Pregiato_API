using Microsoft.EntityFrameworkCore;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class ModelAgencyContext : DbContext
    {
        public ModelAgencyContext(DbContextOptions<ModelAgencyContext> options) : base(options) { }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ClientBilling> ClientsBilling { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Moddels> Models { get; set; }
        public DbSet<Job> Jobs { get; set; }

       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId);
                entity.Property(e => e.ClientId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.ContractId);
                entity.Property(e => e.ContractId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<Moddels>(entity =>
            {
                entity.HasKey(e => e.ModelId);
                entity.Property(e => e.ModelId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.JobId);
                entity.Property(e => e.JobId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });

           
            modelBuilder.Entity<ClientBilling>()
                .HasKey(cb => cb.BillingId);
        

        }
    }
}
