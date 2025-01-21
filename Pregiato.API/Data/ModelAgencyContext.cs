using Microsoft.EntityFrameworkCore;
using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Data
{
    public class ModelAgencyContext : DbContext
    {
        public ModelAgencyContext(DbContextOptions<ModelAgencyContext> options) : base(options) { }

        // DbSets para as entidades
        public DbSet<Client> Clients { get; set; }
        public DbSet<ContractsModels> ContractsModels { get; set; }
        public DbSet<ClientBilling> ClientsBilling { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Moddels> Models { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<ModelsBilling> ModelsBilling { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de ContractsModels
            modelBuilder.Entity<ContractsModels>(entity =>
            {
                entity.HasKey(e => e.ContractId);
                entity.Property(e => e.ContractId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.ContractFile).IsRequired().HasMaxLength(255).HasColumnType("text");
                entity.HasCheckConstraint("CK_ContractsModels_FileFormat", "\"ContractFile\" ~ '\\.(doc|docx|pdf)$'");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configuração de Client
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.IdClient);
                entity.Property(e => e.IdClient)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Contact).HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ClientDocument).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configuração de Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.UserType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configuração de Models
            modelBuilder.Entity<Moddels>(entity =>
            {
                entity.HasKey(e => e.IdModel);
                entity.Property(e => e.IdModel)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CPF).IsRequired().HasMaxLength(14);
                entity.Property(e => e.RG).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PostalCode).HasMaxLength(10);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.BankAccount).HasMaxLength(30);
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.DNA)
                      .HasColumnType("jsonb")
                      .HasDefaultValueSql("'{}'::jsonb");
            });

            // Configuração de Jobs
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.IdJob);
                entity.Property(e => e.IdJob)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configuração de ModelsBilling
            modelBuilder.Entity<ModelsBilling>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd()
                      .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Amount).IsRequired().HasColumnType("numeric(10, 2)");
                entity.Property(e => e.BillingDate).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configuração de LoginUserRequest
            modelBuilder.Entity<LoginUserRequest>(entity =>
            {
                entity.HasNoKey();
            });
        }
    }
}
