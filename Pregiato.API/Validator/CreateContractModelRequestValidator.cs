using FluentValidation;
using Pregiato.API.Requests;

public class CreateContractModelRequestValidator : AbstractValidator<CreateContractModelRequest>
{
    public CreateContractModelRequestValidator()
    {
        RuleFor(x => x.ModelIdentification).NotEmpty().Length(1, 50);
        RuleFor(x => x.UFContract).NotEmpty().Length(2).Matches("^[A-Z]{2}$");
        RuleFor(x => x.City).NotEmpty().Length(2, 100);
        RuleFor(x => x.Day).InclusiveBetween(1, 31);
        RuleFor(x => x.Month).NotEmpty().Must(m => new[] { "janeiro", "fevereiro", "março" /* etc */ }.Contains(m.ToLower()))
                             .WithMessage("Mês inválido.");
        RuleFor(x => x.MonthContract).InclusiveBetween(1, 12);
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