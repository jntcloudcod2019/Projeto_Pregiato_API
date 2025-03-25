namespace Pregiato.API.Response
{
    public class ContractGenerationResponse
    {
        public string ContractName { get; set; }
        public string Message { get; set; }
        public List<ContractSummary> Contracts { get; set; }

        public byte[] ? PdfBytes { get; set; }

    }
    public class ContractSummary
    {
        public int CodProposta { get; set; }
    }
}
