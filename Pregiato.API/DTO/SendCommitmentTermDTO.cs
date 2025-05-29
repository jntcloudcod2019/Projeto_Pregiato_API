namespace Pregiato.API.DTO
{
    public class SendCommitmentTermDTO
    {
        public string? Action { get; set; }
        public Guid IdCommitmentTerm { get; set; }
        public Guid IdModel { get; set; }
        public string? CpfModel { get; set; }

    }
}
