using Microsoft.Build.Framework;

namespace Pregiato.API.Models
{
    public class Job
    {
        public Guid idjob { get; set; }
        [Required]
        public string Description { get; set; }
        public string Status { get; set; } 
        [Required]
        public DateTime JobDate { get; set; }
        [Required]
        public string Location { get; set; }    
    }
}
