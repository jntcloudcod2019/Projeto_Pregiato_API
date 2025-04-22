using Pregiato.API.DTO;
using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface ITrainingRepository
    {
        Task<Training> AddNewTraining (Training newTraining);
        Task<List<Training>> GetAllTrainings();
        Task<Training?> GetTrainingById(Guid id);

        Task SaveOrUpdateProgressAsync(Guid modelId, RegisterLessonProgressDTO dto);
        Task<LessonProgressResponseDTO?> GetProgressAsync(Guid modelId, Guid lessonId);
    }
}
