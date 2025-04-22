namespace Pregiato.API.Models
{
    public class LessonProgress
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public Guid IdModel { get; set; }

        public bool Completed { get; set; }
        public double PercentageWatched { get; set; }

        public DateTime ViewedAt { get; set; }
    }
}
