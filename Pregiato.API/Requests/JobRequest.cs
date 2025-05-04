using Pregiato.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Requests
{
    public class JobRequest
    {
        public string? Description { get; set; }
        public DateTime JobDate { get; set; }
        public string? Location { get; set; }
        public JobStatus? Status { get; set; }
        public decimal Amount { get; set; }
    }
}
