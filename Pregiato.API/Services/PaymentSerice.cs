
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;

public class PaymentService : IPaymentService
{
    public async Task<string> ValidatePayment(PaymentRequest payment)
    {
        try
        {
            if (MetodoPagamento.IsValid == null)
                throw new ArgumentException("Método de pagamento inválido.");

            if (payment.Valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.");

            if (payment.MetodoPagamento == MetodoPagamento.CartaoCredito && payment.QuantidadeParcela == null)
                throw new ArgumentException("Quantidade de parcelas é obrigatória para cartão de crédito.");

            if ((payment.MetodoPagamento == MetodoPagamento.CartaoDebito || payment.MetodoPagamento == MetodoPagamento.CartaoDebito) &&
                string.IsNullOrEmpty(payment.FinalCartao))
                throw new ArgumentException("Os últimos 4 dígitos do cartão são obrigatórios para cartões.");

            if (payment.MetodoPagamento == MetodoPagamento.Pix /*Inserir validação de comprovante*/ )
                throw new ArgumentException("O comprovante é obrigatório para pagamentos via Pix.");

            if (payment.StatusPagamento.ToString() == "Pending" && payment.DataAcordoPagamento == null)
                throw new ArgumentException("A Data do Acordo de Pagamento é obrigatória para status Pending.");

            return "validação de pagamento ok";
        }
        catch (Exception ex)
        {
            ex.InnerException?.ToString();
            return "Ocorreu um problema na validação de pagamentos. Favor contatar time de I.T";
        }
    }
}
