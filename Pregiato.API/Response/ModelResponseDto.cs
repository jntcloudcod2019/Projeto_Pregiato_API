namespace Pregiato.API.Response
{
    public class ModelResponseDto
    {
        public Guid IdModel { get; set; }
        public string Name { get; set; }
        public string CPF { get; set; }
        public string ? RG { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string NumberAddress { get; set; }
        public string Complement { get; set; }
        public string BankAccount { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string UF { get; set; }
        public string TelefonePrincipal { get; set; }
        public string TelefoneSecundario { get; set; }
        public object DNA { get; set; } 
    }
}
