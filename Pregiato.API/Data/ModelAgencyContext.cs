using Microsoft.EntityFrameworkCore;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using System.Xml;

namespace Pregiato.API.Data
{
    public class ModelAgencyContext : DbContext
    {
        public ModelAgencyContext(DbContextOptions<ModelAgencyContext> options) : base(options) { }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ContractsModels> ContractsModels { get; set; }
        public DbSet<ClientBilling> ClientsBilling { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Moddels> Models { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<ModelsBilling> ModelsBilling { get; set; }
        public DbSet<LoginUserRequest> LoginRequests { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<LoginUserRequest>( entity =>
            {
                entity.HasKey( e => e.Username );
                entity.Property( e => e.Username)
                .HasColumnName( "Name" )
                .HasColumnType("text");   
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.IdClient);
                entity.Property(e => e.IdClient)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<ContractsModels>(entity =>
            {
                entity.HasKey(e => e.ContractId);
                entity.Property(e => e.ContractId)  
                     .ValueGeneratedOnAdd()
                     .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.ContractFile)
                .HasColumnType("text")
                .HasColumnType("ContractFile");
               
            });

            modelBuilder.Entity<User>(entity =>
            {
               
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.UserType)
                     .HasConversion<string>();
            });

            modelBuilder.Entity<Moddels>(entity =>
            {
                entity.HasKey(e => e.IdModel);
                entity.Property(e => e.IdModel)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.idjob);
                entity.Property(e => e.idjob)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
            });


            modelBuilder.Entity<ClientBilling>(entity =>
            {
                entity.HasKey(cb => cb.BillingId);
                entity.Property( cb => cb.BillingId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");

            });
            modelBuilder.Entity<ModelsBilling>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

        }
    }
}
