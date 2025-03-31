using FluentValidation;
using Pregiato.API.Requests;

namespace Pregiato.API.Validator;

public class CreateContractModelRequestValidator : AbstractValidator<CreateContractModelRequest>
{
    public CreateContractModelRequestValidator()
    {
        RuleFor(x => x.ModelIdentification).NotEmpty();
        RuleFor(x => x.UFContract).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Day).NotEmpty();
        RuleFor(x => x.Month).NotEmpty();
        RuleFor(x => x.MonthContract).NotEmpty();
        RuleFor(x => x.Payment).NotNull().SetValidator(new PaymentRequestValidator());
    }
}

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.AutorizationNumber).NotEmpty();
        RuleFor(x => x.Valor).NotEmpty();
        RuleFor(x => x.Provider).NotEmpty();
        RuleFor(x => x.StatusPagamento).NotEmpty();
    }
}