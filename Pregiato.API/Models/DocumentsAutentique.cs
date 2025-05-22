using Pregiato.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class DocumentsAutentique
    {
        [Key]
        public string IdDocumentAutentique { get; set; }

        public Guid Id { get; set; }

        public Guid IdContract { get; set; }

        public Guid? IdModel { get; set; }

        public int? CodProposta { get; set; }

        public string? DocumentName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public StatusContratc StatusContratc { get; set; } = StatusContratc.Ativo;
    }
}
