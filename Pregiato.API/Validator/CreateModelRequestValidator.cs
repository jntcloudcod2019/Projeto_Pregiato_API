using FluentValidation;
using Pregiato.API.Requests;

namespace Pregiato.API.Validators
{
    public class CreateModelRequestValidator : AbstractValidator<CreateModelRequest>
    {
        public CreateModelRequestValidator()
        {

            RuleFor(x => x.Nameproducers)
                .NotEmpty().WithMessage("O nome do produtor é obrigatório.");


            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do modelo é obrigatório.");


            RuleFor(x => x.CPF)
                .NotEmpty().WithMessage("O CPF é obrigatório.");


            RuleFor(x => x.RG)
                .NotEmpty().WithMessage("O RG é obrigatório.");


            RuleFor(x => x.DateOfBirth)
                .NotNull().WithMessage("A data de nascimento é obrigatória.");


            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.");


            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("O CEP é obrigatório.");


            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("O endereço é obrigatório.");


            RuleFor(x => x.BankAccount)
                .NotEmpty().WithMessage("A conta bancária é obrigatória.");


            RuleFor(x => x.Neighborhood)
                .NotEmpty().WithMessage("O bairro é obrigatório.");


            RuleFor(x => x.City)
                .NotEmpty().WithMessage("A cidade é obrigatória.");


            RuleFor(x => x.UF)
                .NotEmpty().WithMessage("O estado (UF) é obrigatório.");


            RuleFor(x => x.TelefoneSecundario)
                .NotEmpty().WithMessage("O telefone secundário é obrigatório.");

            RuleFor(x => x.NumberAddress)
                .NotEmpty().WithMessage("O número do endereço é obrigatório.");


            RuleFor(x => x.Complement)
                .NotEmpty().WithMessage("O complemento do endereço é obrigatório.");
        }
    }
}