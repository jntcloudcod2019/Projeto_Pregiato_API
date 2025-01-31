using Microsoft.EntityFrameworkCore;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using System.Diagnostics.Contracts;

namespace Pregiato.API.Data
{
    public class ModelAgencyContext : DbContext
    {
        public ModelAgencyContext(DbContextOptions<ModelAgencyContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ContractsModels> ContractsModels { get; set; }
        public DbSet<ClientBilling> ClientsBilling { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Model> Model { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<ModelsBilling> ModelsBilling { get; set; }
        public DbSet<ContractBase> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ModelJob> ModelJob { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração para a tabela ContractsModels
            modelBuilder.Entity<ContractsModels>()
                .HasKey(c => c.ContractId);

            // Configuração para a tabela Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.MetodoPagamento)
                    .HasColumnType("text") // Altere para 'text'
                    .IsRequired();

                entity.Property(e => e.StatusPagamento)
                    .HasColumnType("text") // Altere para 'text'
                    .IsRequired();

                entity.Property(e => e.Valor).IsRequired();
                entity.Property(e => e.QuantidadeParcela).IsRequired(false);
                entity.Property(e => e.FinalCartao).HasMaxLength(4).IsRequired(false);
                entity.Property(e => e.DataPagamento).IsRequired();
                entity.Property(e => e.Comprovante).IsRequired(false);
                entity.Property(e => e.DataAcordoPagamento).IsRequired(false);

                entity.HasOne<ContractBase>()
                    .WithMany()
                    .HasForeignKey(e => e.ContractId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração para a tabela ModelJob
            modelBuilder.Entity<ModelJob>(entity =>
            {
                entity.HasKey(e => e.ModelJobId);

                entity.Property(e => e.JobDate).IsRequired();
                entity.Property(e => e.Location).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Time).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AdditionalDescription).HasMaxLength(500);

                entity.HasOne(e => e.Model)
                    .WithMany()
                    .HasForeignKey(e => e.ModelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Job)
                    .WithMany()
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração para a tabela ContractBase
            modelBuilder.Entity<ContractBase>(entity =>
            {

                entity.ToTable("Contracts");
                entity.HasKey(c => c.ContractId);
                entity.Property(c => c.CodProposta)
                    .IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasDefaultValue(110);

                entity.Property(c => c.City).IsRequired(false);
                entity.Property(c => c.Neighborhood).IsRequired(false);
                entity.Property(c => c.ContractFilePath).IsRequired(false);
                entity.Property(c => c.Content).HasColumnType("bytea");
                entity.Property(c => c.ValorContrato).IsRequired();
                entity.Property(c => c.FormaPagamento)
                    .HasColumnType("text") // Altere para 'text'
                    .IsRequired();
                entity.Property(c => c.StatusPagamento)
                    .HasColumnType("text") // Altere para 'text'
                    .IsRequired();

                entity.HasOne(c => c.Model)
                       .WithMany(m => m.Contracts)
                       .HasForeignKey(c => c.ModelId)
                       .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<AgencyContract>().ToTable("AgencyContracts");
                modelBuilder.Entity<PhotographyProductionContract>().ToTable("PhotographyProductionContracts");
                modelBuilder.Entity<CommitmentTerm>().ToTable("CommitmentTerms");
                modelBuilder.Entity<ImageRightsTerm>().ToTable("ImageRightsContracts");

                // Configuração para a tabela Models
                modelBuilder.Entity<Model>(entity =>
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

                // Configuração para a tabela Jobs
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

                // Configuração para a tabela ModelsBilling
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

                modelBuilder.Entity<LoginUserRequest>(entity =>
                {
                    entity.HasNoKey();
                });
            });
        }
    }
}
