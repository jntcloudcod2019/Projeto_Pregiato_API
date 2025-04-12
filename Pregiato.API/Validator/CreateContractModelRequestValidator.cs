using FluentValidation;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Validator;

public class CreateContractModelRequestValidator : AbstractValidator<CreateContractModelRequest>
{
    public CreateContractModelRequestValidator()
    {
        RuleFor(x => x.NameProducers).NotEmpty();
        RuleFor(x => x.ModelIdentification).NotEmpty();
        RuleFor(x => x.UFContract).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Day).NotEmpty();
        RuleFor(x => x.Month).NotEmpty();
        RuleFor(x => x.MonthContract).NotEmpty();
        RuleFor(x => x.Payment).SetValidator(new PaymentRequestValidator());
    }
}

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {


        RuleFor(x => x.MetodoPagamento).NotEmpty();

        RuleFor(x => x.AutorizationNumber).NotEmpty()
            .When(x => 
                 x.MetodoPagamento == MetodoPagamento.CartaoCredito || 
                 x.MetodoPagamento == MetodoPagamento.CartaoDebito);

        RuleFor(x => x.Valor).NotEmpty();

        RuleFor(x => x.Provider)
            .NotEmpty()
            .When(x => MetodoExigeProvider(x.MetodoPagamento));

        RuleFor(x => x.StatusPagamento).NotEmpty();

        RuleFor(x => x.DataAcordoPagamento).NotEmpty();

        RuleFor(x => x.DataPagamento).NotEmpty();

        RuleFor(x => x.Provider).NotEmpty()
            .When(x =>
            x.MetodoPagamento == MetodoPagamento.CartaoCredito || 
            x.MetodoPagamento == MetodoPagamento.CartaoDebito);

        RuleFor(x => x.ProofPix)
            .Must(proof => proof != null && proof.Length > 0)
            .When(x => x.MetodoPagamento == MetodoPagamento.Pix)
            .WithMessage("O comprovante do Pix (ProofPix) é obrigatório para pagamentos via Pix.");

    }


    private bool MetodoExigeProvider(MetodoPagamento metodo)
    {
        return metodo == MetodoPagamento.CartaoCredito || metodo == MetodoPagamento.CartaoDebito;
    }
}