using Microsoft.EntityFrameworkCore;
using Pregiato.API.DTO;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Data
{
    public class ModelsRepository : IModelRepository
    {
        private readonly ModelAgencyContext _context;
        public ModelsRepository(ModelAgencyContext context) 
        {
           _context = context;
        }

        public async Task AddModelAsync(Model model)
        {
            if (model.DateOfBirth.HasValue)
            {
                model.DateOfBirth = DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Utc);
            }

            _context.Models.Add(model);  
            await _context.SaveChangesAsync();  
        }

        public async Task DeleteModelAsync(Guid id)
        {
           Model? idModel = await _context.Models.FindAsync(id);
            if (idModel != null)
            { 
                _context.Models.Remove(idModel);    
                await _context.SaveChangesAsync();  
            }
        }

        public async Task<IEnumerable<Model>> GetAllModelAsync()
        {
           return await _context.Models.ToListAsync();  
        }

        public async Task<Model> GetByIdModelAsync(Guid id)
        {

            return await _context.Models.FindAsync(id);
        }

        public async Task UpdateModelAsync(Model model)
        {
           _context.Models.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task<Model?> GetModelByCriteriaAsync(string query)
        {
            return await _context.Models
           .Where(m => m.CPF == query ||
                      m.RG == query ||
                      m.Name.Contains(query) ||
                      m.IdModel.ToString() == query)
           .Select(m => new Model
           {
               IdModel = m.IdModel,
               CPF = m.CPF,
               RG = m.RG,
               Name = m.Name
           })
           .FirstOrDefaultAsync();
        }

        public async Task<Model> GetModelAllAsync(string? idModel, string? cpf, string? rg)
        {
            return await _context.Models.FirstOrDefaultAsync(m =>
                (idModel != null && m.IdModel.ToString() == idModel) ||
                (cpf != null && m.CPF == cpf) ||
                (rg != null && m.RG == rg));       
        }
        public async Task<Model> ModelExistsAsync(CreateModelRequest inputModel)
        {

            Model? existingModel = await _context.Models
                .FirstOrDefaultAsync(m =>
                    m.CPF == inputModel.CPF &&
                    m.Name == inputModel.Name &&
                    m.RG == inputModel.RG &&
                    m.Email == inputModel.Email);
            return existingModel;
        }

        public async Task<ModelCheckDto> GetModelCheck(CreateModelRequest inputModel)
        {
             ModelCheckDto? existingModel = await _context.Models
              .Where(m => m.CPF == inputModel.CPF &&
                         m.Name == inputModel.Name &&
                         m.RG == inputModel.RG &&
                         m.Email == inputModel.Email)
              .Select(m => new ModelCheckDto
              {
                  IdModel = m.IdModel,
                  CPF = m.CPF,
                  Name = m.Name,
                  RG = m.RG,
                  Email = m.Email
              })
              .AsNoTracking()
              .FirstOrDefaultAsync();
            return existingModel;
        }
    }
}
