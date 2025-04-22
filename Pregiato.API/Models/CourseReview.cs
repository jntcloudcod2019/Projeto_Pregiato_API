using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.Models
{
    public class CourseReview
    {
        public Guid Id { get; set; }
        public Guid TrainingId { get; set; }
        public Guid IdModel { get; set; }
        public int Rating { get; set; }
        public string? Feedback { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CommentEntry> Comments { get; set; } = new();
    }

    public class CommentEntry
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
