using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.DTO
{
    [NotMapped]
    public class ModelPhotoUploadDto
    {
        public Guid ModelId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
