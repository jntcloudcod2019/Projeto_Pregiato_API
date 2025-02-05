using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class SalesFeedController : ControllerBase
{
    private readonly ModelAgencyContext _context;

    public SalesFeedController(ModelAgencyContext context)
    {
        _context = context;
    }

    [Authorize(Policy = "AdministratorPolicy")]
    [Authorize(Policy = "Manager")]
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailySales()
    {
        var today = DateTime.UtcNow.Date;

        var sales = await _context.Payments
            .Join(
                _context.Contracts, 
                payment => payment.ContractId,
                contract => contract.ContractId, 
                (payment, contract) => new { Payment = payment, Contract = contract } 
            )
            .Where(x => x.Contract.CreatedAt.Date == today && 
                        (x.Payment.StatusPagamento == "Paid" || x.Payment.StatusPagamento == "Pending")) 
            .SumAsync(x => x.Payment.Valor);

        return Ok(new { TotalSales = sales });
    }

    [Authorize(Policy = "AdministratorPolicy")]
    [Authorize(Policy = "Manager")]
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklySales()
    {
        var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        var sales = await _context.Payments
            .Where(p => p.DataPagamento >= startOfWeek && p.DataPagamento < endOfWeek && (p.StatusPagamento == "Pago" || p.StatusPagamento == "Pending"))
            .SumAsync(p => p.Valor);

        return Ok(new { TotalSales = sales });
    }

    [Authorize(Policy = "AdministratorPolicy")]
    [Authorize(Policy = "Manager")]
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlySales()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var sales = await _context.Payments
            .Join(
                _context.Contracts, 
                payment => payment.ContractId,
                contract => contract.ContractId, 
                (payment, contract) => new { Payment = payment, Contract = contract } 
            )
            .Where(x => x.Contract.CreatedAt >= startOfMonth && x.Contract.CreatedAt < endOfMonth && 
                        (x.Payment.StatusPagamento == "Paid" || x.Payment.StatusPagamento == "Pending"))
            .SumAsync(x => x.Payment.Valor); 

        return Ok(new { TotalSales = sales });
    }
}