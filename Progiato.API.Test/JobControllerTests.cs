using System.Security.Claims;
using Xunit;
using Xunit;
using Moq; 
using Bogus; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Controllers; 
using Pregiato.API.Data; 
using Pregiato.API.Models; 
using Pregiato.API.Requests; 
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Progiato.API.Test
{
    public class JobControllerTests
    {
        private readonly Faker _faker;

        public JobControllerTests()
        {
            _faker = new Faker("pt_BR"); 
        }

        [Fact]
        public async Task GetModelFeed_ShouldReturnPendingJobs()
        {
            var modelId = Guid.NewGuid();
            var fakeJobs = new List<Job>
        {
            new Job
            {
                IdJob = Guid.NewGuid(),
                Status = "Pending",
                JobDate = _faker.Date.Future(),
                Location = _faker.Address.FullAddress(),
                Description = _faker.Lorem.Paragraph()
            },
            new Job
            {
                IdJob = Guid.NewGuid(),
                Status = "Completed", 
                JobDate = _faker.Date.Past(),
                Location = _faker.Address.FullAddress(),
                Description = _faker.Lorem.Paragraph()
            }
        };

            var fakeModelJobs = new List<ModelJob>
        {
            new ModelJob
            {
                ModelJobId = Guid.NewGuid(),
                ModelId = modelId,
                JobId = fakeJobs[0].IdJob,
                AdditionalDescription = _faker.Lorem.Sentence(),
                Time = _faker.Date.Future().ToString("HH:mm")
            }
        };

            // Configura o contexto In-Memory
            var options = new DbContextOptionsBuilder<ModelAgencyContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            await using var context = new ModelAgencyContext(options);

            await context.Jobs.AddRangeAsync(fakeJobs);
            await context.ModelJob.AddRangeAsync(fakeModelJobs);
            await context.SaveChangesAsync();

            var controller = new JobController(context);

            // Simula o usuário autenticado
            var claims = new[] { new Claim("ModelId", modelId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetModelFeed();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var feed = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.NotNull(feed);
           
        }
    }
}