using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class JobRequest
    {
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime JobDate { get; set; }
        [Required]
        public string Location { get; set; }
    }
}
