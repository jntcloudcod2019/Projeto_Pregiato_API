using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;

        public PaymentRepository(IDbContextFactory<ModelAgencyContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Payments.Add(payment);
            await context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment?> GetByIdAsync(Guid paymentId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Payments.FindAsync(paymentId);
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Payments.ToListAsync();
        }

        public async Task<List<Payment>> GetByContractIdAsync(Guid contractId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Payments
                .Where(p => p.ContractId == contractId)
                .ToListAsync();
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Payments.Update(payment);
            await context.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> DeleteAsync(Guid paymentId)
        {
            using var context = _contextFactory.CreateDbContext();
            var payment = await context.Payments.FindAsync(paymentId);
            if (payment == null) return false;

            context.Payments.Remove(payment);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetRevenueOfDayAsync(DateTime? date = null)
        {
            var targetDate = (date ?? DateTime.UtcNow).Date;
            using var context = _contextFactory.CreateDbContext();
            return await context.Payments
                .Where(p => p.DataPagamento.HasValue && p.DataPagamento.Value.Date == targetDate)
                .SumAsync(p => p.Valor);
        }

        public async Task<decimal> GetRevenueOfWeekAsync(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var diff = targetDate.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var startOfWeek = targetDate.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(7);

            using var context = _contextFactory.CreateDbContext();
            return await context.Payments
                .Where(p => p.DataPagamento.HasValue &&
                            p.DataPagamento.Value.Date >= startOfWeek &&
                            p.DataPagamento.Value.Date < endOfWeek)
                .SumAsync(p => p.Valor);
        }

        public async Task<decimal> GetRevenueOfMonthAsync(int? year = null, int? month = null)
        {
            var today = DateTime.UtcNow;
            var targetYear = year ?? today.Year;
            var targetMonth = month ?? today.Month;

            using var context = _contextFactory.CreateDbContext();
            return await context.Payments
                .Where(p => p.DataPagamento.HasValue &&
                            p.DataPagamento.Value.Year == targetYear &&
                            p.DataPagamento.Value.Month == targetMonth)
                .SumAsync(p => p.Valor);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Payments
                .SumAsync(p => p.Valor);
        }

        public async Task<bool> UpdatePaymentDateAsync(Guid paymentId, DateTime newPaymentDate)
        {
            using var context = _contextFactory.CreateDbContext();
            var payment = await context.Payments.FindAsync(paymentId);
            if (payment == null) return false;

            payment.DataPagamento = newPaymentDate;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAgreementDateAsync(Guid paymentId, DateTime newAgreementDate)
        {
            using var context = _contextFactory.CreateDbContext();
            var payment = await context.Payments.FindAsync(paymentId);
            if (payment == null) return false;

            payment.DataAcordoPagamento = newAgreementDate;
            await context.SaveChangesAsync();
            return true;
        }
    }

}
