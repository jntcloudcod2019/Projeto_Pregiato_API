using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interface;
using Pregiato.API.Models;

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
           _context.Model.Add(model);  
            await _context.SaveChangesAsync();  
        }

        public async Task DeleteModelAsync(Guid id)
        {
           var idModel = await _context.Model.FindAsync(id);
            if (idModel != null) 
            { 
                _context.Model.Remove(idModel);    
                await _context.SaveChangesAsync();  
            }
        }

        public async Task<IEnumerable<Model>> GetAllModelAsync()
        {
           return await _context.Model.ToListAsync();  
        }

        public async Task<Model> GetByIdModelAsync(Guid id)
        {

            return await _context.Model.FindAsync(id);
        }

        public async Task UpdateModelAsync(Model model)
        {
           _context.Model.Update(model);
            await _context.SaveChangesAsync();        
        }

        public async Task<Model?> GetModelByCriteriaAsync(string query)
        {
            return await _context.Model.FirstOrDefaultAsync(m =>
                m.CPF == query ||
                m.RG == query ||
                m.Name.Contains(query) ||
                m.IdModel.ToString() == query);
        }

        public async Task<Model> GetModelAllAsync(string? idModel, string? cpf, string? rg)
        {
            return await _context.Model.FirstOrDefaultAsync(m =>
                (idModel != null && m.IdModel.ToString() == idModel) ||
                (cpf != null && m.CPF == cpf) ||
                (rg != null && m.RG == rg));       
        }
    }

}
