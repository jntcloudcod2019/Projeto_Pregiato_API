using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Data;
using Pregiato.API.Services;
using Microsoft.EntityFrameworkCore;

public class PaymentService : IPaymentService
{
    private readonly ModelAgencyContext _context;
    public PaymentService(ModelAgencyContext context) {_context = context ?? throw new ArgumentNullException(nameof(context)); }

    public async Task<string> ValidatePayment(PaymentRequest payment, ContractBase contract)
    {
        try
        {
            if (!MetodoPagamento.IsValid(payment.MetodoPagamento))
                throw new ArgumentException("Método de pagamento inválido.");

            if (payment.Valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.");

            Payment paymentContract = new Payment
            {
                Id = Guid.NewGuid(),
                ContractId = contract.ContractId,
                Valor = contract.ValorContrato,
                DataPagamento = payment.DataPagamento.Value,
                MetodoPagamento = payment.MetodoPagamento,
                StatusPagamento = payment.StatusPagamento
            };

            if (payment.MetodoPagamento == MetodoPagamento.CartaoCredito)
            {
                if (payment.QuantidadeParcela == null)
                    throw new ArgumentException("Quantidade de parcelas é obrigatória para cartão de crédito.");

                if (string.IsNullOrEmpty(payment.FinalCartao))
                    throw new ArgumentException("Os últimos 4 dígitos do cartão são obrigatórios para cartões.");

                paymentContract.QuantidadeParcela = payment.QuantidadeParcela;
                paymentContract.FinalCartao = payment.FinalCartao;
                paymentContract.DataAcordoPagamento = payment.DataAcordoPagamento;
            }

            else if (payment.MetodoPagamento == MetodoPagamento.CartaoDebito)
            {
                if (string.IsNullOrEmpty(payment.FinalCartao))
                    throw new ArgumentException("Os últimos 4 dígitos do cartão são obrigatórios para cartões.");

                paymentContract.FinalCartao = payment.FinalCartao;
            }

            else if (payment.MetodoPagamento == MetodoPagamento.Pix)
            {
                if (payment.Comprovante == null || payment.Comprovante.Length == 0)
                    return "Faça o upload do comprovante após gerar o contrato!";
            }

            else if (payment.MetodoPagamento == MetodoPagamento.LinkPagamento)
            {
                if (string.IsNullOrEmpty(payment.FinalCartao))
                    throw new ArgumentException("Os últimos 4 dígitos do cartão são obrigatórios para Link de Pagamento.");

                paymentContract.FinalCartao = payment.FinalCartao;
            }

            if (payment.StatusPagamento == "Pending" && payment.DataAcordoPagamento == null)
                throw new ArgumentException("A Data do Acordo de Pagamento é obrigatória para status Pending.");

            await _context.AddAsync(paymentContract);
            await _context.SaveChangesAsync();

            return $"validação de pagamento {paymentContract.Id} para o contrato: {payment.ContractId} ok";
        }
        catch (Exception ex)
        {
            return $"Ocorreu um problema na validação de pagamentos: {ex.Message}. Favor contatar time de I.T";
        }
    }
}
