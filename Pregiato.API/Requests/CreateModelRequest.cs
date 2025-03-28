using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pregiato.API.Requests
{
    public class CreateModelRequest
    {
        [Required]
        public string Nameproducers { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string CPF { get; set; }

        [Required]
        public string RG { get; set; }

        [DefaultValue("05-02-2025")]
        [SwaggerSchema("Data de Nascimento")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; } = DateTime.UtcNow;

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string BankAccount { get; set; }

        [Required]
        public string? Neighborhood { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]

        public string UF { get; set; }

        public string TelefonePrincipal { get; set; }
        [Required]
        public string TelefoneSecundario { get; set; }

        [Required]
        public string NumberAddress { get; set; }

        [Required]
        public string Complement { get; set; }

    }
}

