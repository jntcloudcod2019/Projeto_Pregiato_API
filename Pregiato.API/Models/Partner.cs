using ICSharpCode.SharpZipLib.GZip;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Models
{
    public class Partner
    {
        [Key]
        public Guid PartnerId { get; set; }
        [Required]
        public string? PartnerName { get; set; }
        [Required]
        public string? Branch { get; set; }
        [Required]
        public string? Number { get; set; }
        public string? Email { get; set; }
        [Required]
        public string? SocialMidia { get; set; }
        public string? TargetAudience { get; set; }
        public string? ZipCode { get; set; }
        public string? Address { get; set; }
        public string? Complement { get; set; }
        public string? City { get; set; }
        public string? UF { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
