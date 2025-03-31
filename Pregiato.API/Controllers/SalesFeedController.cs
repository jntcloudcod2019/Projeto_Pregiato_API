using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Models;
using Pregiato.API.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Pregiato.API.Interfaces;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;


namespace Pregiato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesFeedController : ControllerBase
{
    private readonly ModelAgencyContext _context;
    private readonly IDbContextFactory<ModelAgencyContext> _contextFactory;
    private readonly IProducersRepository _producersRepository;

    public SalesFeedController(IDbContextFactory<ModelAgencyContext> contextFactory,
        IProducersRepository producersRepository)
    {
        _contextFactory = contextFactory;
        _producersRepository = producersRepository;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailySales()
    {
        
        DateTimeOffset startOfDay = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        DateTimeOffset endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        try
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            var transactions = await context.Payments
                .AsNoTracking()  // Adicionado para melhor performance
                .Where(p => p.DataPagamento >= startOfDay &&
                           p.DataPagamento <= endOfDay &&
                           (p.StatusPagamento == "Paid" || p.StatusPagamento == "Pending"))
                .Select(p => new
                {
                    p.DataPagamento,
                    p.Valor,
                    p.StatusPagamento
                })
                .ToListAsync();

            decimal totalSales = transactions.Sum(t => t.Valor);
            decimal pendingAmount = transactions
                .Where(t => t.StatusPagamento == "Pending")
                .Sum(t => t.Valor);
            decimal paidAmount = transactions
                .Where(t => t.StatusPagamento == "Paid")
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

    //[Authorize(Policy = "ManagementPolicyLevel3")]
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklySales([FromQuery] string date = null!)
    {

        DateTimeOffset startOfWeek;
        if (!string.IsNullOrEmpty(date) && DateTimeOffset.TryParseExact(date, "yyyy-MM-dd", null,
                global::System.Globalization.DateTimeStyles.None, out DateTimeOffset parsedDate))
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
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            var transactions = await context.Payments
                .Where(p => p.DataPagamento >= startOfWeek && p.DataPagamento <= endOfWeek &&
                            (p.StatusPagamento == "Paid" || p.StatusPagamento == "Pending"))
                .Select(c => new
                {
                    c.DataPagamento,
                    c.Valor,
                    c.StatusPagamento
                })
                .ToListAsync().ConfigureAwait(true);

            decimal totalSales = transactions.Sum(t => t.Valor);
            decimal pendingAmount = transactions.Where(t => t.StatusPagamento == "Pending")
                .Sum(t => t.Valor);
            decimal paidAmount = transactions.Where(t => t.StatusPagamento == "Paid")
                .Sum(t => t.Valor);

            return Ok(new BillingResponse
            {
                Success = true,
                Message =
                    $"Faturamento da semana de {startOfWeek.ToString("dd-MM-yyyy")} a {endOfWeek.ToString("dd-MM-yyyy")}.",
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

   // [Authorize(Policy = "ManagementPolicyLevel3")]
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlySales()
    {
        DateTimeOffset startOfMonth =
            new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        try
        {
            using ModelAgencyContext context = _contextFactory.CreateDbContext();

            var transactions = await context.Payments
                .Where(p => p.DataPagamento >= startOfMonth && p.DataPagamento < endOfMonth &&
                            (p.StatusPagamento == "Paid" || p.StatusPagamento == "Pending"))
                .Select(c => new
                {
                    c.DataPagamento,
                    c.Valor,
                    c.StatusPagamento
                })
                .ToListAsync().ConfigureAwait(true);

            decimal totalSales = transactions.Sum(t => t.Valor);

            decimal pendingAmount = transactions.Where(t => t.StatusPagamento == "Pending")
                .Sum(t => t.Valor);

            decimal paidAmount = transactions.Where(t => t.StatusPagamento == "Paid")
                .Sum(t => t.Valor);

            return Ok(new BillingResponse
            {
                Success = true,
                Message = $"Faturamento do  mês  de {startOfMonth.ToString("MMMM")}",
                Data = new BillingData
                {
                    TotalSales = totalSales,
                    Currency = "BRL",
                    Period = new Period
                    {
                        StartDate = startOfMonth.ToString("dd-MM-yyyy"),
                        EndDate = endOfMonth.ToString("dd-MM-yyyy ")
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

    [Authorize(Policy = "ManagementPolicyLevel3")]
    [HttpGet("GetBillingDayByProducers")]
    public async Task<IActionResult> GetBillingDayByProducers()
    {
        try
        {
            User user = await UserCaptureByToken();
            List<Producers> producers = await _producersRepository.GetDailyBillingByProducers(user);

            if (producers == null || !producers.Any())
            {
                return Ok(new BillingResponseProducers
                {
                    SUCESS = false,
                    MESSAGE = "Nenhum registro encontrado para o dia atual",
                    DATA = null,
                    RESUME = null
                });
            }

            var billingDataList = producers.Select(p =>
            {
                ModelDetails modelDetails = null;
                if (p.InfoModel != null)
                {
                    var detailsInfo = (DetailsInfo)p.InfoModel;
                    modelDetails = new ModelDetails
                    {
                        IdModel = detailsInfo.DocumentModel,
                        NameModel = detailsInfo.NameModel,
                        DocumentModel = detailsInfo.DocumentModel
                    };
                }

                return new BillingDataProducers
                {
                    NAMEPRODUCERS = p.NameProducer,
                    AMOUNTCONTRACT = p.AmountContract,
                    DATE = p.CreatedAt.ToString("dd/MM/yyyy"),
                    STATUSCONTRACT = p.StatusContratc.ToString(),
                    MODELDETAILS = modelDetails
                };
            }).ToList();
            var resumeData = new BillingDataResume
            {
                TOTASTALESCONTRACT = producers.Sum(p => p.AmountContract),
                TOTALCONTRACTS = producers.Count
            };

            return Ok(new BillingResponseProducers
            {
                SUCESS = true,
                MESSAGE = "SEU RENDIMENTO DE HOJE:",
                DATA = billingDataList,
                RESUME = resumeData
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

   // [Authorize(Policy = "ManagementPolicyLevel3")]
    [HttpGet("GetAllBillingDayProducers")]
    public async Task<IActionResult> GetAllBillingDayProducers( )
    {
        try
        {
            List<Producers> producers = await _producersRepository.GetBillingDayProducers();

            
                if (producers == null || !producers.Any())
                {
                    return Ok(new BillingResponseProducers
                    {
                        SUCESS = false,
                        MESSAGE = "Nenhum registro encontrado para o dia atual",
                        DATA = null,
                        RESUME = null
                    });

                }

            var groupedProducers = producers
                .GroupBy(p => p.NameProducer)
                .Select(g => new
                {
                    NameProducer = g.Key,
                    TotalSalesSum = g.Sum(p => p.AmountContract), 
                    TransactionCount = g.Count(), 
                    Producers = g.ToList() 
                });

            
            var billingDataList = groupedProducers
                .SelectMany(g => g.Producers.Select(p => new BillingDataProducers
                {
                    NAMEPRODUCERS = p.NameProducer,
                    AMOUNTCONTRACT = p.AmountContract,
                    TRANSACTIONSCOUNT = g.TransactionCount, 
                    DATE = p.CreatedAt.ToString("dd/MM/yyyy"),
                    STATUSCONTRACT = p.StatusContratc.ToString(),
                    DAYSALESSUM = g.TotalSalesSum 
                }))
                .OrderBy(p => p.NAMEPRODUCERS)
                .ToList();

            var resumeData = new BillingDataResume
            {
                TOTASTALESCONTRACT = producers.Sum(p => p.AmountContract),
                TOTALCONTRACTS = producers.Count
            };

            return Ok(new BillingResponseProducers
            {
                SUCESS = true,
                MESSAGE = "RENDIMENTO DIÁRIO DOS PRODUTORES:",
                DATA = billingDataList,
                RESUME = resumeData,
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
                }
            });
        }
    }

    private async Task<User> UserCaptureByToken()
       {
        var authorizationHeader = HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
            {
                Message = "TOKEN INVÁLIDO",
            }));
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken;

        try
        {
            jwtToken = handler.ReadJwtToken(token);
        }
        catch (Exception)
        {
            throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
            {
                Message = "TOKEN INVÁLIDO",
            }));
        }

        string GetClaimValue(string claimType)
        {
            return jwtToken.Claims.FirstOrDefault(c =>
                c.Type == claimType ||
                c.Type.EndsWith($"/{claimType}", StringComparison.OrdinalIgnoreCase))?.Value;
        }

        var userId = GetClaimValue("nameid") ?? GetClaimValue(ClaimTypes.NameIdentifier);
        var userName = GetClaimValue("unique_name") ?? GetClaimValue(ClaimTypes.Name);
        var email = GetClaimValue("email") ?? GetClaimValue(ClaimTypes.Email);
        var userType = GetClaimValue("role") ?? GetClaimValue(ClaimTypes.Role);

        using ModelAgencyContext context = _contextFactory.CreateDbContext();

        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Email.ToString() == email);

        if (user == null)
        {
            user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == userName || u.Email == email);
        }

        if (user == null)
        {
            throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
            {
                Message = "USUÁRIO NÃO ENCONTRADO NA BASE DE DADOS",
            }));
        }

        return user;
    }
}