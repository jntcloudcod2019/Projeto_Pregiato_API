namespace Pregiato.API.DTO
{
    public class CreateLessonDTO
    {
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
    }
}
