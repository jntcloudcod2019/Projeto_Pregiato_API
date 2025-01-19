using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public JobController(IJobRepository jobRepository, IModelRepository modelRepository)
        {
            _jobRepository = jobRepository;
            _modelRepository = modelRepository;
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
              idjob = new Guid(),
              JobDate = jobRequest.JobDate,
              Location = jobRequest.Location,
              Description = jobRequest.Description,                      
             };               
            await _jobRepository.AddAJobsync(jobModel);  
            return Ok(jobModel);                         
        }
    }
}
