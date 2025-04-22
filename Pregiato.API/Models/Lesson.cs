namespace Pregiato.API.Models
{
    public class Lesson
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string VideoUrl { get; set; } = string.Empty;

        public int Likes { get; set; }
        public int Dislikes { get; set; }

        public Guid TrainingId { get; set; }
    }
}
