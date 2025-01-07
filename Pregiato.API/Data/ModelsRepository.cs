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

        public async Task AddModelAsync(Moddels model)
        {
           _context.Models.Add(model);  
            await _context.SaveChangesAsync();  
        }

        public async Task DeleteModelAsync(Guid id)
        {
           var idModel = await _context.Models.FindAsync(id);
            if (idModel != null) 
            { 
                _context.Models.Remove(idModel);    
                await _context.SaveChangesAsync();  
            }
        }

        public async Task<IEnumerable<Moddels>> GetAllModelAsync()
        {
           return await _context.Models.ToListAsync();  
        }

        public async Task<Moddels> GetByIdModelAsync(Guid id)
        {
            return await _context.Models.FindAsync(id);
        }

        public async Task UpdateModelAsync(Moddels model)
        {
           _context.Models.Update(model);
            await _context.SaveChangesAsync();        
        }
    }

}
