using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Jobs")]
    public class JobController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;
        private readonly IModelRepository _modelRepository;
        private readonly ModelAgencyContext _agencyContext;

        public JobController(IJobRepository jobRepository, IModelRepository modelRepository, ModelAgencyContext agencyContext)
        {
            _jobRepository = jobRepository;
            _modelRepository = modelRepository;
            _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));
        }



        [Authorize(Roles = "AdministratorPolicy,ManagerPolicy")]
        [HttpPost]
        [SwaggerOperation("Criação de Job")]
        public async Task <IActionResult> AddJobModel( [FromBody] JobRequest jobRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var jobModel = new Job
            {
              Status = "Confirmed",
              IdJob = new Guid(),
              JobDate = jobRequest.JobDate,
              Location = jobRequest.Location,
              Description = jobRequest.Description,                      
             };               
            await _jobRepository.AddAJobsync(jobModel);  
            return Ok(jobModel);                         
        }

        [Authorize(Roles = "AdministratorPolicy,ManagerPolicy")]
        [HttpPost("assign-job-to-model")]
        public async Task<IActionResult> AssignJobToModel([FromBody] AssignJobToModelRequest request)
        {

            if (request == null)
            {
                return BadRequest("Requisição inválida.");
            }


            if (request.ModelId == Guid.Empty || request.JobId == Guid.Empty)
            {
                return BadRequest("ModelId e JobId são obrigatórios.");
            }

            var model = await _agencyContext.Model.FindAsync(request.ModelId);
            if (model == null)
            {
                return NotFound($"Modelo com ID {request.ModelId} não encontrado.");
            }


            var job = await _agencyContext.Jobs.FindAsync(request.JobId);
            if (job == null)
            {
                return NotFound($"Job com ID {request.JobId} não encontrado.");
            }


            var modelJob = new ModelJob
            {
                ModelJobId = Guid.NewGuid(),
                ModelId = request.ModelId,
                JobId = request.JobId,
                JobDate = request.JobDate,
                Location = request.Location,
                Time = request.Time,
                AdditionalDescription = request.AdditionalDescription,
                Status = "Pending"
            };

            // Adiciona ao contexto
            await _agencyContext.ModelJob.AddAsync(modelJob);
            await _agencyContext.SaveChangesAsync();

            return Ok("Job atribuído ao modelo com sucesso.");
        }

    }
}
