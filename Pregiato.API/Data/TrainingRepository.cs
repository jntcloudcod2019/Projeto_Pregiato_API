using Microsoft.EntityFrameworkCore;
using Pregiato.API.DTO;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class TrainingRepository : ITrainingRepository
    {
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;
        public TrainingRepository(IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Training> AddNewTraining(Training newTraining)
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            context.Set<Training>().Add(newTraining);
            await context.SaveChangesAsync();

            return newTraining;
        }

        public async Task<List<Training>> GetAllTrainings()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<Training>().AsNoTracking().ToListAsync();
        }

        public  async Task<Training?> GetTrainingById(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<Training>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task SaveOrUpdateProgressAsync(Guid modelId, RegisterLessonProgressDTO dto)
        {
            using var context = _contextFactory.CreateDbContext();

            var existing = await context.LessonProgresses
                .FirstOrDefaultAsync(p => p.IdModel == modelId && p.LessonId == dto.LessonId);

            if (existing == null)
            {
                var progress = new LessonProgress
                {
                    Id = Guid.NewGuid(),
                    IdModel = modelId,
                    LessonId = dto.LessonId,
                    PercentageWatched = dto.PercentageWatched,
                    Completed = dto.PercentageWatched >= 90,
                    ViewedAt = DateTime.UtcNow
                };

                context.LessonProgresses.Add(progress);
            }
            else
            {
                existing.PercentageWatched = Math.Max(existing.PercentageWatched, dto.PercentageWatched);
                existing.Completed = existing.PercentageWatched >= 90;
                existing.ViewedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        public async Task<LessonProgressResponseDTO?> GetProgressAsync(Guid modelId, Guid lessonId)
        {
            using var context = _contextFactory.CreateDbContext();

            var result = await context.LessonProgresses
                .AsNoTracking()
                .Where(p => p.IdModel == modelId && p.LessonId == lessonId)
                .Select(p => new LessonProgressResponseDTO
                {
                    LessonId = p.LessonId,
                    Completed = p.Completed,
                    PercentageWatched = p.PercentageWatched,
                    ViewedAt = p.ViewedAt
                })
                .FirstOrDefaultAsync();

            return result;
        }


    }
}
