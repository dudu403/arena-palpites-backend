using Bolao.Api.Contracts.Predictions;
using FluentValidation;

namespace Bolao.Api.Validators.Predictions;

public sealed class CreatePredictionRequestValidator
    : AbstractValidator<CreatePredictionRequest>
{
    public CreatePredictionRequestValidator()
    {
        RuleFor(x => x.BolaoId)
            .NotEmpty()
            .WithMessage("Bolão é obrigatório.");

        RuleFor(x => x.FootballMatchId)
            .NotEmpty()
            .WithMessage("Partida é obrigatória.");

        RuleFor(x => x.HomeScore)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Placar do mandante não pode ser negativo.");

        RuleFor(x => x.AwayScore)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Placar do visitante não pode ser negativo.");

        RuleFor(x => x.HomeScore)
            .LessThanOrEqualTo(30)
            .WithMessage("Placar inválido.");

        RuleFor(x => x.AwayScore)
            .LessThanOrEqualTo(30)
            .WithMessage("Placar inválido.");
    }
}