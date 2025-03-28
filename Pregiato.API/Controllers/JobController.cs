using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("creatJob")]
        [SwaggerOperation("Criação de Job")]
        public async Task <IActionResult> AddJobModel( [FromBody] JobRequest jobRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"Erro ao criar Job, favor reviar os campos de preenchimento.");
            }

            var jobModel = new Job
            {
              Status = "Pending",
             IdModel = new Guid(),
              JobDate = jobRequest.JobDate,
              Location = jobRequest.Location,
              Description = jobRequest.Description,                      
             };               
            await _jobRepository.AddAJobsync(jobModel);  
            return Ok(jobModel);                         
        }

        [Authorize(Policy = "AdminOrManager")]
        [HttpPost("assign-job-to-model")]
        public async Task<IActionResult> AssignJobToModel([FromBody] AssignJobToModelRequest request)
        {

            if (request == null)
            {
                return BadRequest("Requisição inválida.");
            }


            if (request.ModelId == Guid.Empty || request.JobId == Guid.Empty)
            {
                return BadRequest("ModelId ou JobId são obrigatórios.");
            }

            var model = await _agencyContext.Models.FindAsync(request.ModelId);
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
               
                IdModel= request.ModelId,
                JobDate = request.JobDate,
                Location = request.Location,
                Time = request.Time,
                AdditionalDescription = request.AdditionalDescription,
            };

            await _agencyContext.ModelJobs.AddAsync(modelJob);
            await _agencyContext.SaveChangesAsync();

            return Ok("Job atribuído ao modelo com sucesso.");
        }

    }
}
