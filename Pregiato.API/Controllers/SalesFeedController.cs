using Microsoft.AspNetCore.Mvc;
using Pregiato.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Pregiato.API.Response;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
public class SalesFeedController : ControllerBase
{
    private readonly ModelAgencyContext _context;

    public SalesFeedController(ModelAgencyContext context)
    {
        _context = context;
    }

    [Authorize(Policy = "AdminOrManager")]
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailySales()
    {
        DateTimeOffset startOfDay = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero); 
        DateTimeOffset endOfDay = startOfDay.AddDays(1).AddTicks(-1); 

        try
        {
            var transactions = await _context.Payments
                .Where(p => p.DataPagamento >= startOfDay && p.DataPagamento <= endOfDay &&
                            (p.StatusPagamento == "Paid" || p.StatusPagamento == "Pending"))
                .Select(c => new
                {
                    c.DataPagamento,
                    c.Valor,
                    c.StatusPagamento
                })
                .ToListAsync();

            var totalSales = transactions.Sum(t => t.Valor);
            var pendingAmount = transactions.Where(t => t.StatusPagamento == "Pending")
                                           .Sum(t => t.Valor);
            var paidAmount = transactions.Where(t => t.StatusPagamento == "Paid")
                                        .Sum(t => t.Valor);

            return Ok(new BillingResponse
            {
                Success = true,
                Message = $"Faturamento do dia {startOfDay.ToString("dd-MM-yyyy")}.",
                Data = new BillingData
                {
                    TotalSales = totalSales,
                    Currency = "BRL",
                    Period = new Period
                    {
                        StartDate = startOfDay.ToString("dd-MM-yyyy"),
                        EndDate = endOfDay.ToString("dd-MM-yyyy")
                    },
                    TransactionsCount = transactions.Count,
                    PendingAmount = pendingAmount,
                    PaidAmount = paidAmount
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Ocorreu um erro ao calcular o faturamento diário.",
                error = new
                {
                    code = "INTERNAL_SERVER_ERROR",
                    details = ex.Message
                }
            });
        }
    }

    [Authorize(Policy = "AdminOrManager")]
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklySales([FromQuery] string date = null)
    {
       
        DateTimeOffset startOfWeek;
        if (!string.IsNullOrEmpty(date) && DateTimeOffset.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            startOfWeek = new DateTimeOffset(parsedDate.Date, TimeSpan.Zero); 
        }
        else
        {
            startOfWeek = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero); 
        }

        DateTimeOffset endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);

        try
        {
            var transactions = await _context.Payments
                .Where(p => p.DataPagamento >= startOfWeek && p.DataPagamento <= endOfWeek &&
                            (p.StatusPagamento == "Paid" || p.StatusPagamento == "Pending"))
                .Select(c => new
                {
                    c.DataPagamento,
                    c.Valor,
                    c.StatusPagamento
                })
                .ToListAsync();

            var totalSales = transactions.Sum(t => t.Valor);
            var pendingAmount = transactions.Where(t => t.StatusPagamento == "Pending")
                                           .Sum(t => t.Valor);
            var paidAmount = transactions.Where(t => t.StatusPagamento == "Paid")
                                        .Sum(t => t.Valor);

            return Ok(new BillingResponse
            {
                Success = true,
                Message = $"Faturamento da semana de {startOfWeek.ToString("dd-MM-yyyy")} a {endOfWeek.ToString("dd-MM-yyyy")}.",
                Data = new BillingData
                {
                    TotalSales = totalSales,
                    Currency = "BRL",
                    Period = new Period
                    {
                        StartDate = startOfWeek.ToString("dd-MM-yyyy"),
                        EndDate = endOfWeek.ToString("dd-MM-yyyy")
                    },
                    TransactionsCount = transactions.Count,
                    PendingAmount = pendingAmount,
                    PaidAmount = paidAmount
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Ocorreu um erro ao calcular o faturamento semanal.",
                error = new
                {
                    code = "INTERNAL_SERVER_ERROR",
                    details = ex.Message
                }
            });
        }
    }

    [Authorize(Policy = "AdminOrManager")]
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlySales()
    {
        DateTimeOffset startOfMonth = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        try
        {
            var transactions = await _context.Payments
                .Where(p => p.DataPagamento >= startOfMonth && p.DataPagamento < endOfMonth &&
                            (p.StatusPagamento == "Paid" || p.StatusPagamento == "Pending"))
                .Select(c => new
                {
                    c.DataPagamento,
                    c.Valor ,
                    c.StatusPagamento
                })
                .ToListAsync();

           var totalSales = transactions.Sum(t => t.Valor);

           var pendingAmount = transactions.Where(t => t.StatusPagamento == "Pending")
                                                  .Sum(t => t.Valor);

           var paidAmount = transactions.Where(t => t.StatusPagamento == "Paid")
                                               .Sum(t => t.Valor);        

            return Ok(new BillingResponse
            {
                Success = true,
                Message = $"Faturamento do  mês  de {startOfMonth.Month.ToString("MMMM")}. ",
                Data = new BillingData
                {
                    TotalSales = totalSales,
                    Currency = "BRL",
                    Period = new Period
                    {
                        StartDate = startOfMonth.ToString("dd-MM-yyyy"),
                        EndDate =endOfMonth.ToString("dd-MM-yyyy ")
                    },
                    TransactionsCount = transactions.Count,
                    PendingAmount = pendingAmount,
                    PaidAmount = paidAmount
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Ocorreu um erro ao calcular o faturamento mensal.",
                error = new
                {
                    code = "INTERNAL_SERVER_ERROR",
                    details = ex.Message
                }
            });
        }
    }
}