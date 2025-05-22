namespace Pregiato.API.Response
{
    public class ContractResponse
    {
        public int StatusCode { get; set; }
        public string? ContractFilePath { get; set; }
        public int? CodProposta { get; set; }
        public decimal ValorContrato { get; set; }
        public DateTime DataContrato { get; set; }
    }
}
