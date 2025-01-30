using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interface
{
    public interface IPaymentService
    {
       Task <string> ValidatePayment(PaymentRequest payment);
    }
}
