namespace Pregiato.API.Models
{
    public class Training
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string?  Description { get; set; }
        public int TotalLessons => Lessons?.Count ?? 0;
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
