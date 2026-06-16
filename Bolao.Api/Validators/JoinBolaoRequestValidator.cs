using Bolao.Api.Contracts.Boloes;
using FluentValidation;

namespace Bolao.Api.Validators.Boloes;

public class JoinBolaoRequestValidator : AbstractValidator<JoinBolaoRequest>
{
    public JoinBolaoRequestValidator()
    {
        RuleFor(x => x.InviteCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O código do bolão é obrigatório.")
            .Length(6, 10)
            .WithMessage("O código do bolão é inválido.")
            .Matches("^[A-Za-z0-9]+$")
            .WithMessage("O código do bolão contém caracteres inválidos.");
    }
}