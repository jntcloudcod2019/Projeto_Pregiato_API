using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class ModelJob
    {

        [Key]
        public Guid ModelJobId { get; set; } = Guid.NewGuid();
        public Guid ModelId { get; set; }
        public Guid JobId { get; set; }
        public DateTime JobDate { get; set; }
        [Required, MaxLength(255)]
        public string Location { get; set; }
        [Required, MaxLength(50)]
        public string Time { get; set; }
        [MaxLength(500)]
        public string AdditionalDescription { get; set; }

        [ForeignKey("ModelId")]
        public Moddels Model { get; set; }

        [ForeignKey("JobId")]
        public Job Job { get; set; }

        public string Status { get; set; }
    }
}
