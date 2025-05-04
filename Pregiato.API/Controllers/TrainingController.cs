using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pregiato.API.Data;
using Pregiato.API.DTO;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Response;
using Pregiato.API.Services;
using System.Security.Claims;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingController : Controller
    {
        private readonly ITrainingRepository _repository;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        public TrainingController(ITrainingRepository repository, IDbContextFactory<ModelAgencyContext> contextFactory, IUserService userService, IUserRepository userRepository)
        {
            _repository = repository;
            _contextFactory = contextFactory;
            _userService = userService;
            _userRepository = userRepository;
        }

        [HttpPost("CreateTraining")]
        public async Task<IActionResult> CreateTraining([FromBody] CreateTrainingDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("NOME E INSTRUTOR SÃO OBRIGATÓRIOS.");
            }

            var training = new Training
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                InstructorName = dto.Name,
                Description = dto.Description
            };

            if (dto.Lessons != null && dto.Lessons.Any())
            {
                foreach (var lessonDto in dto.Lessons)
                {
                    training.Lessons.Add(new Lesson
                    {
                        Id = Guid.NewGuid(),
                        Title = lessonDto.Title,
                        VideoUrl = lessonDto.VideoUrl,
                        Duration = lessonDto.Duration,
                        TrainingId = training.Id 
                    });
                }
            }

            var created = await _repository.AddNewTraining(training);

            var response = new TrainingResponse
            {
                ID = created.Id,
                NAME = created.Name,
                INSTRUCTOR = created.InstructorName,
                DESCRIPTION = created.Description,
             
            };

            return StatusCode(201, response);
        }


        [HttpPost("Lessons/likes/{lessonId}")]
        public async Task<IActionResult> RegisterLike(Guid lessonId, [FromQuery] bool like = true)
        {
            using var context = _contextFactory.CreateDbContext();

            var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return NotFound("Aula não encontrada.");

            if (like)
                lesson.Likes++;
            else
                lesson.Dislikes++;

            await context.SaveChangesAsync();

            return Ok(new
            {
                message = like ? "Like registrado!" : "Dislike registrado!",
                likes = lesson.Likes,
                dislikes = lesson.Dislikes
            });
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpPost("Lessons/RegisterProcess")]
        public async Task<IActionResult> RegisterProgress([FromBody] RegisterLessonProgressDTO dto)
        {

            var username = await _userService.UserCaptureByToken().ConfigureAwait(true);

            var schforModel = await _userRepository.GetByUsernameAsync(username.Email).ConfigureAwait(true);
            if (schforModel is null)
            {
                return BadRequest("USUÁRIO NÃO ENCONTRADO NA BASE DE DADOS.");
            }
            using var context = _contextFactory.CreateDbContext();

            var model = await context.Models
                .FirstOrDefaultAsync(m => m.Email == schforModel.Email || m.CodProducers == schforModel.CodProducers)
                .ConfigureAwait(true);

            await _repository.SaveOrUpdateProgressAsync(model.IdModel, dto);
            return Ok(new { message = "PROGRESSO REGISTRADO COM SUCESSO." });
        }

        [Authorize(Policy = "GlobalPoliticsAgency")]
        [HttpGet("Lessons/ProgressReturn/{lessonId}")]
        public async Task<IActionResult> GetProgress(Guid lessonId)
        {
            var username = await _userService.UserCaptureByToken().ConfigureAwait(true);

            var schforModel = await _userRepository.GetByUsernameAsync(username.Email).ConfigureAwait(true);
            if (schforModel is null)
            {
                return BadRequest("USUÁRIO NÃO ENCONTRADO NA BASE DE DADOS.");
            }

            using var context = await _contextFactory.CreateDbContextAsync().ConfigureAwait(true);

            var model = await context.Models
                .FirstOrDefaultAsync(m => m.Email == schforModel.Email || m.CodProducers == schforModel.CodProducers)
                .ConfigureAwait(true);

            var progress = await _repository.GetProgressAsync(model.IdModel, lessonId);

            if (progress == null)
                return NotFound("NENHUM PROGRESSO ENCONTRADO.");

            return Ok(progress);
        }

    }
}
