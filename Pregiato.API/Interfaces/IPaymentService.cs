using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interface
{
    public interface IPaymentService
    {
       Task <string> ValidatePayment( Producers producers,PaymentRequest payment, ContractBase contractBase);
    }
}
