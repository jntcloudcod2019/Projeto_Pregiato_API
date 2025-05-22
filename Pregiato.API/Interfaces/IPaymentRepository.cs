using Pregiato.API.Models;

namespace Pregiato.API.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(Payment payment);
        Task<Payment?> GetByIdAsync(Guid paymentId);
        Task<List<Payment>> GetAllAsync();
        Task<List<Payment>> GetByContractIdAsync(Guid contractId);
        Task<Payment> UpdateAsync(Payment payment);
        Task<bool> DeleteAsync(Guid paymentId);

        Task<decimal> GetRevenueOfDayAsync(DateTime? date = null);
        Task<decimal> GetRevenueOfWeekAsync(DateTime? date = null);
        Task<decimal> GetRevenueOfMonthAsync(int? year = null, int? month = null);
        Task<decimal> GetTotalRevenueAsync();

        Task<bool> UpdatePaymentDateAsync(Guid paymentId, DateTime newPaymentDate);
        Task<bool> UpdateAgreementDateAsync(Guid paymentId, DateTime newAgreementDate);
    }
}
