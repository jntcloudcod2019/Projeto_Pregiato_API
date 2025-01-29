using Pregiato.API.Interface;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class ModelsBillingRepository : IModesBilling
    {
        private readonly ModelAgencyContext _context;   
        private ModelsBillingRepository(ModelAgencyContext context) 
        {
          _context = context;   
        }

        public async Task AddModelsBillingAsync(ModelsBilling modelsBilling)
        {
            _context.ModelsBilling.Add(modelsBilling);
            await _context.SaveChangesAsync();
            await Task.CompletedTask;
        }

        public async Task DeleteModelsBillingAsync(Guid id)
        {
            var billing = _context.ModelsBilling.FirstOrDefault(b => b.IdModel== id);
            if (billing != null)
            {
                _context.ModelsBilling.Remove(billing);
            }
            await Task.CompletedTask;
        }

        public Task<IEnumerable<ModelsBilling>> GetAllModelsBillingAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ModelsBilling> GetByIdModelsBillingAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateModelsBillingAsync(ModelsBilling modelsBilling)
        {
            var existingBilling = _context.ModelsBilling.FirstOrDefault(b => b.IdModel == modelsBilling.IdModel);
            if (existingBilling != null)
            {
               existingBilling.Amount = modelsBilling.Amount;   
                existingBilling.BillingDate = modelsBilling.BillingDate;

            }
            await Task.CompletedTask;
        }
    }
}
