namespace Pregiato.API.Services.ServiceModels
{
    public class ContractMessage
    {
        public List<Guid> ContractIds { get; set; }
        public string CpfModel { get; set; }
        public string Timestamp { get; set; }
    }
}
