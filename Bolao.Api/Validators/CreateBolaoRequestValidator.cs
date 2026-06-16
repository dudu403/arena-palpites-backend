using Bolao.Api.Contracts.Boloes;
using FluentValidation;

namespace Bolao.Api.Validators.Boloes;

public class CreateBolaoRequestValidator : AbstractValidator<CreateBolaoRequest>
{
    private static readonly string[] AllowedPrivacy =
    [
        "Private",
        "Public"
    ];

    private static readonly string[] AllowedChampionships =
    [
        "Copa do Mundo"
    ];

    public CreateBolaoRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O nome do bolão é obrigatório.")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("O nome do bolão é obrigatório.")
            .MinimumLength(3)
            .WithMessage("O nome do bolão deve ter pelo menos 3 caracteres.")
            .MaximumLength(80)
            .WithMessage("O nome do bolão deve ter no máximo 80 caracteres.")
            .Must(NotContainHtml)
            .WithMessage("O nome do bolão contém conteúdo inválido.");

        RuleFor(x => x.Championship)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O campeonato é obrigatório.")
            .Must(x => AllowedChampionships.Contains(x))
            .WithMessage("Campeonato inválido.");

        RuleFor(x => x.ChampionshipExternalId)
            .Equal(72)
            .WithMessage("Campeonato inválido.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("A descrição deve ter no máximo 200 caracteres.")
            .Must(x => string.IsNullOrWhiteSpace(x) || NotContainHtml(x))
            .WithMessage("A descrição contém conteúdo inválido.");

        RuleFor(x => x.MaxParticipants)
            .InclusiveBetween(2, 60)
            .WithMessage("O número de participantes deve estar entre 2 e 60.");

        RuleFor(x => x.Privacy)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("A privacidade é obrigatória.")
            .Must(x => AllowedPrivacy.Contains(x))
            .WithMessage("Privacidade inválida.");

        RuleFor(x => x.Rules)
            .NotNull()
            .WithMessage("As regras do bolão são obrigatórias.");

        When(x => x.Rules != null, () =>
        {
            RuleFor(x => x.Rules.ExactScorePoints)
                .InclusiveBetween(0, 100)
                .WithMessage("Pontuação de placar exato inválida.");

            RuleFor(x => x.Rules.WinnerPoints)
                .InclusiveBetween(0, 100)
                .WithMessage("Pontuação de vencedor inválida.");

            RuleFor(x => x.Rules.DrawPoints)
                .InclusiveBetween(0, 100)
                .WithMessage("Pontuação de empate inválida.");

            RuleFor(x => x.Rules)
                .Must(r => r.ExactScorePoints > r.WinnerPoints)
                .WithMessage("Placar exato deve valer mais que acerto de vencedor.");

            RuleFor(x => x.Rules)
                .Must(r => r.ExactScorePoints > r.DrawPoints)
                .WithMessage("Placar exato deve valer mais que acerto de empate.");

            RuleFor(x => x.Rules)
                .Must(r => r.DrawPoints > r.WinnerPoints)
                .WithMessage("Acerto de empate deve valer mais que acerto de vencedor.");
        });
    }

    private static bool NotContainHtml(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var normalized = value.Trim().ToLowerInvariant();

        string[] blocked =
        [
            "<",
            ">",
            "<script",
            "</script",
            "javascript:",
            "onerror=",
            "onload=",
            "iframe"
        ];

        return !blocked.Any(normalized.Contains);
    }
}