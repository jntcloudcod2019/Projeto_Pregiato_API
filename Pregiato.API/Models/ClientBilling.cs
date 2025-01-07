namespace Pregiato.API.Models
{
    public class ClientBilling
    {
        public int BillingId { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillingDate { get; set; }
    }
}
