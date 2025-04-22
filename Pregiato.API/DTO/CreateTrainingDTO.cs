using Microsoft.AspNetCore.Mvc;

namespace Pregiato.API.DTO
{
    public class CreateTrainingDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<CreateLessonDTO>? Lessons { get; set; }
    }
}
