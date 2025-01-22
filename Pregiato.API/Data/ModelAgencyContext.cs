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
        public DbSet<ContractBase> Contracts { get; set; } // Tabela base

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ModelJob>(entity =>
            {
                entity.HasKey(e => e.ModelJobId);

                entity.Property(e => e.JobDate)
                      .IsRequired();

                entity.Property(e => e.Location)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Time)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.AdditionalDescription)
                      .HasMaxLength(500);

                // Configura o relacionamento com a tabela Models
                entity.HasOne(e => e.Model)
                      .WithMany()
                      .HasForeignKey(e => e.ModelId)
                      .OnDelete(DeleteBehavior.Cascade); // Remove os registros relacionados ao excluir o modelo

                // Configura o relacionamento com a tabela Jobs
                entity.HasOne(e => e.Job)
                      .WithMany()
                      .HasForeignKey(e => e.JobId)
                      .OnDelete(DeleteBehavior.Cascade); // Remove os registros relacionados ao excluir o job
            });

            modelBuilder.Entity<ContractBase>(entity =>
            {
                entity.HasKey(c => c.ContractId);
                entity.Property(c => c.City).IsRequired(false);
                entity.Property(c => c.Neighborhood).IsRequired(false);
                entity.Property(c => c.ContractFilePath).IsRequired(false);
                entity.Property(c => c.Content).HasColumnType("bytea");

                // Configuração do campo CodProposta
                modelBuilder.Entity<ContractBase>(entity =>
                {
                    entity.HasKey(c => c.ContractId);
                    entity.Property(c => c.CodProposta)
                          .IsRequired()
                          .ValueGeneratedOnAdd()
                          .HasDefaultValue(110); // Começa em 110
                    entity.ToTable("Contracts");
                });
            });

            modelBuilder.Entity<Moddels>(entity =>
            {
                entity.Property(e => e.City).IsRequired(false); // Permite valores nulos
                entity.Property(e => e.Neighborhood).IsRequired(false); // Bairro opcional
            });

            // Configuração para a tabela base (ContractBase)
            modelBuilder.Entity<ContractBase>(entity =>
            {
                entity.HasKey(c => c.ContractId); // Define a chave primária
                entity.Property(c => c.City).IsRequired(false);
                entity.Property(c => c.Neighborhood).IsRequired(false);
                entity.Property(c => c.ContractFilePath).IsRequired(false);
                entity.Property(c => c.Content).HasColumnType("bytea");
                entity.ToTable("Contracts"); // Nome da tabela base
            });

            // Configuração para as tabelas derivadas (TPT)
            modelBuilder.Entity<AgencyContract>().ToTable("AgencyContracts");
            modelBuilder.Entity<PhotographyProductionContract>().ToTable("PhotographyProductionContracts");
            modelBuilder.Entity<CommitmentTerm>().ToTable("CommitmentTerms");
            modelBuilder.Entity<ImageRightsTerm>().ToTable("ImageRightsContracts");

            // Configuração de Clients
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

                // Adiciona a coluna DNA como JSON
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
