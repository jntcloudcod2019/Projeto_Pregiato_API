using Microsoft.EntityFrameworkCore;
using Pregiato.API.Data;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;
using System.Globalization;
using Microsoft.OpenApi.Writers;

namespace Pregiato.API.Services;

public class PaymentService(IDbContextFactory<ModelAgencyContext> contextFactory) : IPaymentService
{
    private readonly IDbContextFactory<ModelAgencyContext> _contextFactory =
        contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

    public async Task<string> ValidatePayment(Producers producers, PaymentRequest payment, ContractBase contract)
    {
        await using ModelAgencyContext context = await _contextFactory.CreateDbContextAsync();
        try
        {
            if (!MetodoPagamento.IsValid(payment.MetodoPagamento))
                throw new ArgumentException("Método de pagamento inválido.");

            if (payment.Valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.");
            var valorString = payment.Valor != 0
                ? payment.Valor.ToString(CultureInfo.InvariantCulture)
                : "0";

            Payment paymentContract = new Payment
            {
                ContractId = contract.ContractId,
                PaymentId = contract.PaymentId,
                CodProducers = producers.CodProducers,
                Valor = decimal.Parse(
                    valorString.Replace("R$", "")
                        .Replace(" ", "")
                        .Replace(".", "")
                        .Replace(",", "."),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture),

                DataPagamento = payment.DataPagamento!.Value.ToUniversalTime(),
                MetodoPagamento = payment.MetodoPagamento,
                StatusPagamento = StatusPagamento.Create(payment.StatusPagamento),
                AutorizationNumber = payment.AutorizationNumber,
                Provider = payment.Provider,
            };

            if (payment.MetodoPagamento == MetodoPagamento.CartaoCredito)
            {
                if (payment.QuantidadeParcela == null)
                    throw new ArgumentException("Quantidade de parcelas é obrigatória para cartão de crédito.");

                if (string.IsNullOrEmpty(payment.FinalCartao))
                    throw new ArgumentException("Os últimos 4 dígitos do cartão são obrigatórios para cartões.");

                paymentContract.QuantidadeParcela = payment.QuantidadeParcela;
                paymentContract.FinalCartao = payment.FinalCartao;
                paymentContract.DataAcordoPagamento = payment.DataAcordoPagamento.Value.ToUniversalTime();
            }

            else if (payment.MetodoPagamento == MetodoPagamento.CartaoDebito)
            {
                if (!string.IsNullOrEmpty(payment.FinalCartao))
                {
                    if (string.IsNullOrEmpty(payment.AutorizationNumber))
                        throw new ArgumentException("É necessário  informar  o número de autorização.");
                    if (payment.Provider == null)
                        throw new ArgumentException("É necessário  informar o provedor da maquina.");

                    paymentContract.FinalCartao = payment.FinalCartao;
                }
                else
                {
                    throw new ArgumentException("Os últimos 4 dígitos do cartão são obrigatórios para cartões.");
                }
            }

            else if (payment.MetodoPagamento == MetodoPagamento.Pix)
            {
                if(payment.ProofPix == null)
                    return "Faça o upload do comprovante após gerar o contrato!";
                paymentContract.Comprovante = payment.ProofPix;
                paymentContract.QuantidadeParcela = payment.QuantidadeParcela;
                paymentContract.FinalCartao = payment.FinalCartao ?? "N/A";
                paymentContract.DataAcordoPagamento = payment.DataAcordoPagamento.Value.ToUniversalTime();

            }

            else if (payment.MetodoPagamento == MetodoPagamento.LinkPagamento)
            {
                //if (string.IsNullOrEmpty(payment.FinalCartao))
                //    throw new ArgumentException(
                //        "Os últimos 4 dígitos do cartão são obrigatórios para Link de Pagamento.");

                paymentContract.FinalCartao = payment.FinalCartao;
                paymentContract.QuantidadeParcela = payment.QuantidadeParcela;
                paymentContract.FinalCartao = payment.FinalCartao ?? "N/A";
                paymentContract.DataAcordoPagamento = payment.DataAcordoPagamento.Value.ToUniversalTime();
            }

            if (payment.StatusPagamento == "Pending" && payment.DataAcordoPagamento == null)
                throw new ArgumentException("Data do Acordo de Pagamento é obrigatória para status Pendente.");

            await context.AddAsync(paymentContract);
            await context.SaveChangesAsync();

            return $"validação de pagamento  para o contrato: {contract.ContractId} ok";
        }
        catch (Exception ex)
        {
            return $"Ocorreu um problema na validação de pagamentos: {ex.Message}. Favor contatar time de I.T";
        }
    }
}