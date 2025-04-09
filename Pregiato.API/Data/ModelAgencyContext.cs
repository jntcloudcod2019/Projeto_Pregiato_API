using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pregiato.API.DTO;
using Pregiato.API.Enums;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Data
{
    public class ModelAgencyContext : DbContext
    {

        public ModelAgencyContext(DbContextOptions<ModelAgencyContext> options) : base(options) { }
        public DbSet<ContractsModels> ContractsModels { get; set; }
        public DbSet<User?> Users { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<ContractBase> Contracts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ModelJob> ModelJobs { get; set; }
        public DbSet<Producers> Producers { get; set; }
        public DbSet<ModelsBilling> ModelsBilling { get; set; } 
        public DbSet<ModelPhoto> ModelPhotos { get; set; }

        public override int SaveChanges()
        {
            IEnumerable<EntityEntry> entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Model && e.State == EntityState.Modified);

            foreach (EntityEntry entry in entries)
            {
                
                ((Model)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
           
            modelBuilder.Ignore<LoginUserRequest>();
            modelBuilder.Ignore<ContractDTO>();

            // Índices
            modelBuilder.Entity<Model>().HasIndex(m => m.CPF);
            modelBuilder.Entity<User>().HasIndex(u => u.CodProducers);
            modelBuilder.Entity<User>().HasIndex(u => u.Name);
            modelBuilder.Entity<ModelPhoto>().HasIndex(u => u.ModelId);

            modelBuilder.Entity<ContractBase>()
                .ToTable("Contracts")
                .HasDiscriminator<string>("ContractType")
                .HasValue<AgencyContract>("Agency")
                .HasValue<PhotographyProductionContract>("PhotographyProduction")
                .HasValue<CommitmentTerm>("Commitment")
                .HasValue<ImageRightsTerm>("ImageRights")
                .HasValue<PhotographyProductionContractMinority>("PhotographyMinority");


            // Configuração de Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                ValueConverter<StatusPagamento, string> statusPagamentoConverter = new ValueConverter<StatusPagamento, string>(
                    v => v.Value,
                    v => StatusPagamento.Create(v));

                EnumToStringConverter<ProviderEnum> providerConverter = new EnumToStringConverter<ProviderEnum>();

                entity.ToTable("Payments");

                entity.Property(e => e.StatusPagamento)
                    .HasConversion(statusPagamentoConverter)
                    .HasColumnType("text")
                    .IsRequired();

                entity.Property(e => e.Provider)
                    .HasConversion(providerConverter)
                    .HasColumnType("text")
                    .IsRequired(false);
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.Valor).IsRequired(true);
                entity.Property(e => e.QuantidadeParcela).IsRequired(true);
                entity.Property(e => e.FinalCartao).HasMaxLength(10).IsRequired(false);
                entity.Property(e => e.DataPagamento)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.Comprovante).HasColumnType("bytea");
                entity.Property(e => e.DataAcordoPagamento)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP") 
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.MetodoPagamento).IsRequired(true);
                entity.Property(e => e.AutorizationNumber).IsRequired(true);

            });

            // Configuração de Producers
            modelBuilder.Entity<Producers>(entity =>
            {
                entity.ToTable("Producers");
                entity.HasKey(e => e.CodProducers);
                entity.Property(e => e.NameProducer).IsRequired(true);
                entity.Property(e => e.CodProposal).IsRequired(true);
                entity.Property(e => e.AmountContract).IsRequired().IsRequired(true);
                entity.Property(e => e.StatusContratc)
                    .HasConversion<string>();
                entity.Property(e => e.TotalAgreements).HasDefaultValue(1);
                entity.Property(e => e.InfoModel)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                        v => JsonSerializer.Deserialize<DetailsInfo>(v, new JsonSerializerOptions())!);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP") 
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("Jobs");
                entity.HasKey(e => e.JobId);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.Amount).IsRequired(true);
                entity.Property(e => e.Location).IsRequired(false);
                entity.Property(e => e.JobDate).IsRequired();
                entity.Property(e => e.Description).IsRequired(false);

            });


            modelBuilder.Entity<ModelsBilling>(entity =>
            {
                entity.ToTable("ModelsBilling");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired(true);
                entity.Property(e => e.BillingDate);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP") 
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");

            });

            modelBuilder.Entity<User>().ToTable("Users");


            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Email).IsRequired(true);
                entity.Property(e => e.Name).IsRequired(true);
                entity.Property(e => e.NickName).IsRequired(true);
                entity.Property(e => e.PasswordHash).IsRequired(true);
                entity.Property(e => e.UserType).IsRequired(true);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");


            });

            // Configuração de ModelJob
            modelBuilder.Entity<ModelJob>(entity =>
            {
                entity.ToTable("ModelJob");
                entity.Property(e => e.Id)
                    .HasColumnType("uuid")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobDate);
                
                entity.Property(e => e.Location)
                    .IsRequired(false)
                    .HasMaxLength(255);
                entity.Property(e => e.Time)
                    .IsRequired(false);
                entity.Property(e => e.AdditionalDescription)
                    .HasMaxLength(500);
                
            });

            modelBuilder.Entity<ModelPhoto>(entity =>
            {
                entity.ToTable("model_photos");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(e => e.ModelId)
                    .HasColumnName("model_id")
                    .IsRequired();

                entity.Property(e => e.ImageData)
                    .HasColumnName("image_data")
                    .IsRequired();

                entity.Property(e => e.ImageName)
                    .HasColumnName("image_name")
                    .HasMaxLength(255);

                entity.Property(e => e.ContentType)
                    .HasColumnName("content_type")
                    .HasMaxLength(100);

                entity.Property(e => e.UploadedAt)
                    .HasColumnName("uploaded_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });



            // Configuração de ContractBase
            modelBuilder.Entity<ContractBase>(entity =>
            {
                entity.ToTable("Contracts");
                entity.HasKey(e => e.ContractId);
                entity.Property(e => e.CodProposta)
                    .IsRequired()
                    .HasDefaultValue(400);
                entity.Property(e => e.ContractFilePath).IsRequired(true);
                entity.Property(e => e.Content).HasColumnType("bytea");
                entity.Property(e => e.ValorContrato).IsRequired();
                entity.Property(e => e.FormaPagamento)
                    .HasColumnType("text")
                    .IsRequired();
                entity.Property(e => e.StatusPagamento)
                    .HasColumnType("text")
                    .IsRequired();
                entity.Property(e => e.StatusContratc)
                    .HasConversion<string>();
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone"); 
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone"); 
            });



            // Configuração de Model
            modelBuilder.Entity<Model>(entity =>
            {
                entity.ToTable("Models");
                entity.HasKey(e => e.IdModel);
                entity.Property(e => e.IdModel)
                    .HasColumnName("ModelId")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CPF).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RG).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DateOfBirth)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.Age).IsRequired(true);
                entity.Property(e => e.Complement).IsRequired(true);
                entity.Property(e => e.Status).IsRequired(true);
                entity.Property(e => e.Neighborhood).IsRequired(true);
                entity.Property(e => e.UF).IsRequired(true);
                entity.Property(e => e.TelefonePrincipal).IsRequired(true);
                entity.Property(e => e.PostalCode).IsRequired();
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.BankAccount).HasMaxLength(30);
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")  
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.DNA);
            });
        }
    }
}