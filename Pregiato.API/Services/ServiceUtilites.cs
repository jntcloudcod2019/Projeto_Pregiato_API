using Pregiato.API.Interface;
using System.Globalization;

namespace Pregiato.API.Services
{
    public class ServiceUtilites : IServiceUtilites
    {
        public async Task<int> CalculateAge(DateTime dateOfBirth)
        {
            if (dateOfBirth == null)
                throw new ArgumentException("Data de nascimento é obrigatória.");

            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;
          
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
