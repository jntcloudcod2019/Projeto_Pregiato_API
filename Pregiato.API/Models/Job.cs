namespace Pregiato.API.Models
{
    public class Job
    {
        public int JobId { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; }
        public string Description { get; set; }
    }
}
