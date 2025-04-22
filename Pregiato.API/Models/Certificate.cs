namespace Pregiato.API.Models
{
    public class Certificate
    {
        public Guid Id { get; set; }

        public Guid IdModel { get; set; }

        public Guid TrainingId { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public string FileName { get; set; } = string.Empty; 

        public byte[] PdfBytes { get; set; } = [];
    }
}
