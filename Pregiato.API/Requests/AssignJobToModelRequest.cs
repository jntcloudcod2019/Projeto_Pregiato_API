using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class AssignJobToModelRequest
    {
        [Required]
        public Guid ModelId { get; set; }

        [Required]
        public Guid JobId { get; set; }

        [SwaggerSchema("Data Job")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime JobDate { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "A localização deve ter no máximo 255 caracteres.")]
        public string Location { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "O horário deve ter no máximo 50 caracteres.")]
        public string Time { get; set; }

        [StringLength(500, ErrorMessage = "A descrição adicional deve ter no máximo 500 caracteres.")]
        public string? AdditionalDescription { get; set; }
    }
}
