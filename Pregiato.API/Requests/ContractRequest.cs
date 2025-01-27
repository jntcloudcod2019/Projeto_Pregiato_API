namespace Pregiato.API.Requests
{
    public class ContractRequest
    {
        public Guid JobId { get; set; }

        public List<PaymentRequest> Payments { get; set; } = new List<PaymentRequest>();
    }
}
