using Org.BouncyCastle.Asn1.Mozilla;
using Pregiato.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class Job
    {
        [Key]
        public Guid JobId { get; set; }
        [Required]
        public Guid PartnerId { get; set; }
        public string? Description { get; set; }
        [Required]
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public DateTime JobDate { get; set; }
        public string? Location { get; set; }
        public decimal Amount { get; set; }
        public string? Partnership { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
