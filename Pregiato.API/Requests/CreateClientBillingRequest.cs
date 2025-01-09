namespace Pregiato.API.Requests
{
    public class ClientBillingRequest
    {
        public decimal? Amount { get; set; }
        public DateTime? BillingDate { get; set; }
    }
}
