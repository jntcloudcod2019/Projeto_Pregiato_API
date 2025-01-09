namespace Pregiato.API.Models
{
    public class ClientBilling
    {
        public Guid BillingId { get; set; }
        public Guid ClientId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? BillingDate { get; set; }
    }
}
