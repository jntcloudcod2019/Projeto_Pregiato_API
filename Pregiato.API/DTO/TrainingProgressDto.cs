using System.ComponentModel.DataAnnotations.Schema;

namespace Pregiato.API.DTO
{
    [NotMapped]
    public class TrainingProgressDto
    {
        public Guid TrainingId { get; set; }
        public int LessonsCompleted { get; set; }
        public int TotalLessons { get; set; }
        public double Percentage => TotalLessons == 0 ? 0 : ((double)LessonsCompleted / TotalLessons) * 100;
    }
}
