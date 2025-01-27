using Pregiato.API.Models;

namespace Pregiato.API.Interface
{
    public interface IPaymentService
    {
       Task <string> ValidatePayment(Payment payment);
    }
}
