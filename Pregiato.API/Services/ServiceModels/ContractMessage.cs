namespace Pregiato.API.Services.ServiceModels
{
    public class ContractMessage
    {
        public string? Action { get; set; }
        public string? IdDocumentAutentique { get; set; }
        public Guid? IdContract { get; set; }
        public List<Guid> ContractIds { get; set; }
        public string? CpfModel { get; set; }
        public string? Timestamp { get; set; }
    }
}
