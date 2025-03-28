using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Models
{
    public class Model
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid IdModel { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        public string? Name { get; set; }

        [JsonIgnore]
        public List<ContractBase> Contracts { get; set; } = new List<ContractBase>();

        [Required]
        [StringLength(14, ErrorMessage = "CPF deve ter no máximo 14 caracteres.")]
        public string? CPF { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "RG deve ter no máximo 20 caracteres.")]
        public string? RG { get; set; }

        [Required]
        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data de Nascimento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        [JsonIgnore]
        public int Age { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(10, ErrorMessage = "CEP deve ter no máximo 10 caracteres.")]
        public string? PostalCode { get; set; }

        [StringLength(255, ErrorMessage = "Endereço deve ter no máximo 255 caracteres.")]
        public string? Address { get; set; }

        [Required]
        public string? NumberAddress { get; set; }

        [Required]
        public string ? Complement{ get; set; }

        [StringLength(30, ErrorMessage = "Conta bancária deve ter no máximo 30 caracteres.")]
        public string? BankAccount { get; set; }

        [Required]
        [JsonIgnore]
        public bool Status { get; set; } = default == true;

        [Required]
        public string? Neighborhood { get; set; }
        [Required]
        public string? City { get; set; }

        [Required]
        public string? UF { get; set; }

        [Required]
        public string? TelefonePrincipal { get; set; }
        [Required]
        public string? TelefoneSecundario { get; set; }

        [JsonIgnore]
        public JsonDocument? DNA { get; set; }

        [Required]
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(10)]
        [Required]
        public string? CodProducers { get; set; }

        public ICollection<ContractBase>? Contract { get; set; }

        public ICollection<ModelJob>? ModelJobs { get; set; }

        public ModelsBilling ModelsBilling { get; set; }

    }
}
