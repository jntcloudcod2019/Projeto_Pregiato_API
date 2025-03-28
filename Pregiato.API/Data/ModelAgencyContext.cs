using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using Pregiato.API.DTO;
using Pregiato.API.Enums;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;
using PuppeteerSharp;

namespace Pregiato.API.Data
{
    public class ModelAgencyContext : DbContext
    {


        public ModelAgencyContext(DbContextOptions<ModelAgencyContext> options) : base(options) { }


        public DbSet<ContractsModels> ContractsModels { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<ContractBase> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ModelJob> ModelJobs { get; set; }
        public DbSet<Producers> Producers { get; set; }
        public DbSet<ModelsBilling> ModelsBilling { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            // Ignora entidades não persistidas
            modelBuilder.Ignore<LoginUserRequest>();
            modelBuilder.Entity<ContractDTO>().HasNoKey();

            // Índices
            modelBuilder.Entity<Model>().HasIndex(m => m.CPF);
            modelBuilder.Entity<User>().HasIndex(u => u.CodProducers);

            // Configuração de Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                var statusPagamentoConverter = new ValueConverter<StatusPagamento, string>(
                    v => v.Value,
                    v => StatusPagamento.Create(v));

                var providerConverter = new EnumToStringConverter<ProviderEnum>();

                entity.ToTable("Payments");

                entity.Property(e => e.StatusPagamento)
                    .HasConversion(statusPagamentoConverter)
                    .HasColumnType("text")
                    .IsRequired();

                entity.Property(e => e.Provider)
                    .HasConversion(providerConverter)
                    .HasColumnType("text")
                    .IsRequired();

                entity.Property(e => e.Valor).IsRequired();
                entity.Property(e => e.QuantidadeParcela).IsRequired(false);
                entity.Property(e => e.FinalCartao).HasMaxLength(4).IsRequired(false);
                entity.Property(e => e.DataPagamento).IsRequired();
                entity.Property(e => e.Comprovante).IsRequired(false);
                entity.Property(e => e.DataAcordoPagamento).IsRequired(false);

              
            });

            // Configuração de Producers
            modelBuilder.Entity<Producers>(entity =>
            {
                entity.HasKey(p => p.IdProducer);

                entity.Property(e => e.StatusContratc)
                    .HasConversion<string>();

                entity.Property(p => p.InfoModel)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                        v => JsonSerializer.Deserialize<DetailsInfo>(v, new JsonSerializerOptions())!);

                entity.Property(p => p.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configuração de ModelJob
            modelBuilder.Entity<ModelJob>(entity =>
            {
                entity.Property(e => e.JobDate).IsRequired();
                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.Time)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.AdditionalDescription)
                    .HasMaxLength(500);

                entity.HasOne(e => e.Model)
                    .WithMany(m => m.ModelJobs)
                    .HasForeignKey(e => e.IdModel)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Job)
                    .WithMany(j => j.ModelJobs)
                    .HasForeignKey(e => e.IdJob)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração de ContractBase
            modelBuilder.Entity<ContractBase>(entity =>
            {
                entity.ToTable("Contracts");
                entity.HasKey(c => c.ContractId);

                entity.Property(c => c.CodProposta)
                    .IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasDefaultValue(110);
                entity.Property(c => c.ContractFilePath).IsRequired(false);
                entity.Property(c => c.Content).HasColumnType("bytea");
                entity.Property(c => c.ValorContrato).IsRequired();
                entity.Property(c => c.FormaPagamento)
                    .HasColumnType("text")
                    .IsRequired();
                entity.Property(c => c.StatusPagamento)
                    .HasColumnType("text")
                    .IsRequired();
                entity.Property(c => c.StatusContratc)
                    .HasConversion<string>();

                entity.HasOne(c => c.Model)
                    .WithMany(m => m.Contracts)
                    .HasForeignKey(c => c.ModelId)
                    .OnDelete(DeleteBehavior.Cascade);

            
            });

            // Configuração de ModelsBilling
            modelBuilder.Entity<ModelsBilling>()
                .HasOne(mb => mb.Model)
                .WithOne(m => m.ModelsBilling)
                .HasForeignKey<ModelsBilling>(mb => mb.IdModel);

            // Configuração de Model
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

            // Configuração das classes derivadas de ContractBase (TPH)
            modelBuilder.Entity<AgencyContract>().ToTable("Contracts");
            modelBuilder.Entity<PhotographyProductionContract>().ToTable("Contracts");
            modelBuilder.Entity<CommitmentTerm>().ToTable("Contracts");
            modelBuilder.Entity<ImageRightsTerm>().ToTable("Contracts");
            modelBuilder.Entity<PhotographyProductionContractMinority>().ToTable("Contracts");
        }
    }
}