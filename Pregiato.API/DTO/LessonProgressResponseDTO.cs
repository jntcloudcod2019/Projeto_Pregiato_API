namespace Pregiato.API.DTO
{
    public class LessonProgressResponseDTO
    {
        public Guid LessonId { get; set; }
        public bool Completed { get; set; }
        public double PercentageWatched { get; set; }
        public DateTime ViewedAt { get; set; }
    }
}
