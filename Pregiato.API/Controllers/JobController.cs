using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Data;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Pregiato.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController(IJobRepository jobRepository, ModelAgencyContext agencyContext)
        : ControllerBase
    {
        private readonly ModelAgencyContext _agencyContext = agencyContext ?? throw new ArgumentNullException(nameof(agencyContext));

        [Authorize(Policy = "ManagementPolicyLevel5")]
        [HttpPost("CreatJob")]
        [SwaggerOperation("Criação de Job")]
        public async Task <IActionResult> AddJobModel( [FromBody] JobRequest jobRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest($"ERRO AO CRIAR JOB, FAVOR REVISAR OS CAMPOS DE PREENCHIMENTO.");
            }

            Job jobModel = new Job
            {
              Status = "Pending",
              JobDate = jobRequest.JobDate,
              Location = jobRequest.Location,
              Description = jobRequest.Description,                      
             };               
            await jobRepository.AddAJobsync(jobModel).ConfigureAwait(true);  
            return Ok(jobModel);                         
        }

        [Authorize(Policy = "ManagementPolicyLevel5")]
        [HttpPost("Assign-job-to-model")]
        public async Task<IActionResult> AssignJobToModel([FromBody] AssignJobToModelRequest request)
        {

            if (request == null)
            {
                return BadRequest("REQUISIÇÃO INVÁLIDA.");
            }


            if (request.ModelId == Guid.Empty || request.JobId == Guid.Empty)
            {
                return BadRequest("MODELID OU JOBID SÃO OBRIGATÓRIOS.");
            }

            Model? model = await _agencyContext.Models.FindAsync(request.ModelId)
                                                      .ConfigureAwait(true);
            if (model == null)
            {
                return NotFound($"MODELO COM ID {request.ModelId} NÃO ENCONTRADO.");
            }

            Job? job = await _agencyContext.Jobs.FindAsync(request.JobId)
                                                .ConfigureAwait(true);

            if (job == null)
            {
                return NotFound($"JOB COM ID {request.JobId} NÃO ENCONTRADO.");
            }

            ModelJob modelJob = new ModelJob
            {
               
                ModelId= request.ModelId,
                JobDate = request.JobDate,
                Location = request.Location,
                Time = request.Time,
                AdditionalDescription = request.AdditionalDescription,
            };

            await _agencyContext.ModelJobs.AddAsync(modelJob)
                                          .ConfigureAwait(true);
            await _agencyContext.SaveChangesAsync()
                                .ConfigureAwait(true);

            return Ok("JOB ATRIBUÍDO AO MODELO COM SUCESSO.");
        }

    }
}
