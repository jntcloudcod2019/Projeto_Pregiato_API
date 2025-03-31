using Pregiato.API.Interfaces;

namespace Pregiato.API.Services
{
    public class ServiceUtilites : IServiceUtilites
    {
        public async Task<int> CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;
          
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return await Task.FromResult(age).ConfigureAwait(false);
        }

    }
}
