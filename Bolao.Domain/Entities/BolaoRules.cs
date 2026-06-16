namespace Bolao.Domain.Entities;

public class BolaoRules
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid BolaoId { get; private set; }

    public Bolao Bolao { get; private set; } = null!;

    public int ExactScorePoints { get; private set; }

    public int WinnerPoints { get; private set; }

    public int DrawPoints { get; private set; }

    private BolaoRules() { }

    public BolaoRules(
        int exactScorePoints,
        int winnerPoints,
        int drawPoints)
    {
        ExactScorePoints = ValidatePoints(
            exactScorePoints,
            nameof(ExactScorePoints));

        WinnerPoints = ValidatePoints(
            winnerPoints,
            nameof(WinnerPoints));

        DrawPoints = ValidatePoints(
            drawPoints,
            nameof(DrawPoints));

        ValidateConsistency();
    }

    private static int ValidatePoints(
        int value,
        string field)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException($"{field} deve estar entre 0 e 100.");

        return value;
    }

    private void ValidateConsistency()
    {
        if (ExactScorePoints <= WinnerPoints)
            throw new ArgumentException(
                "Placar exato deve valer mais que acerto de vencedor.");

        if (ExactScorePoints <= DrawPoints)
            throw new ArgumentException(
                "Placar exato deve valer mais que acerto de empate.");

        if (DrawPoints <= WinnerPoints)
            throw new ArgumentException(
                "Acerto de empate deve valer mais que acerto de vencedor.");
    }
}