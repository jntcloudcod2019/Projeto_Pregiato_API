namespace Pregiato.API.Requests
{
    public class UpdateModelRequest
    {
        public string? Name { get; set; }
        public string? CPF { get; set; }
        public string? RG { get; set; }
        public string? Email { get; set; }
        public string? PostalCode { get; set; }
        public string? Address { get; set; }
        public string? BankAccount { get; set; }
    }
}
