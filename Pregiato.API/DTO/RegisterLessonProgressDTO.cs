namespace Pregiato.API.DTO
{
    public class RegisterLessonProgressDTO
    {
        public Guid LessonId { get; set; }
        public double PercentageWatched { get; set; }
    }
}
