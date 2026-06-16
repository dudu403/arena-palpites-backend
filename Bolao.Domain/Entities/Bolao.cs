namespace Bolao.Domain.Entities;

public class Bolao
{
    public const string PrivatePrivacy = "Private";
    public const string PublicPrivacy = "Public";

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;

    public string Championship { get; private set; } = string.Empty;

    public int ChampionshipExternalId { get; private set; }

    public string? Description { get; private set; }

    public int MaxParticipants { get; private set; }

    public string Privacy { get; private set; } = string.Empty;

    public string? InviteCode { get; private set; }

    public Guid OwnerId { get; private set; }

    public User Owner { get; private set; } = null!;

    public BolaoRules Rules { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    private Bolao()
    {
    }

    public Bolao(
        string name,
        string championship,
        int championshipExternalId,
        string? description,
        int maxParticipants,
        string privacy,
        Guid ownerId,
        string? inviteCode,
        BolaoRules rules)
    {
        SetName(name);
        SetChampionship(championship);
        SetChampionshipExternalId(championshipExternalId);
        SetDescription(description);
        SetMaxParticipants(maxParticipants);
        SetPrivacy(privacy);
        SetOwner(ownerId);
        SetInviteCode(inviteCode);

        Rules = rules ?? throw new ArgumentException("As regras do bolão são obrigatórias.");
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do bolão é obrigatório.");

        name = name.Trim();

        if (name.Length < 3 || name.Length > 80)
            throw new ArgumentException("O nome do bolão deve ter entre 3 e 80 caracteres.");

        Name = name;
    }

    private void SetChampionship(string championship)
    {
        if (string.IsNullOrWhiteSpace(championship))
            throw new ArgumentException("O campeonato é obrigatório.");

        championship = championship.Trim();

        if (championship.Length > 80)
            throw new ArgumentException("O campeonato deve ter no máximo 80 caracteres.");

        Championship = championship;
    }

    private void SetChampionshipExternalId(int championshipExternalId)
    {
        if (championshipExternalId <= 0)
            throw new ArgumentException("Campeonato inválido.");

        ChampionshipExternalId = championshipExternalId;
    }

    private void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
            return;
        }

        description = description.Trim();

        if (description.Length > 200)
            throw new ArgumentException("A descrição deve ter no máximo 200 caracteres.");

        Description = description;
    }

    private void SetMaxParticipants(int maxParticipants)
    {
        if (maxParticipants is < 2 or > 60)
            throw new ArgumentException("O número de participantes deve estar entre 2 e 60.");

        MaxParticipants = maxParticipants;
    }

    private void SetPrivacy(string privacy)
    {
        if (privacy != PrivatePrivacy && privacy != PublicPrivacy)
            throw new ArgumentException("Privacidade inválida.");

        Privacy = privacy;
    }

    private void SetOwner(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Dono do bolão inválido.");

        OwnerId = ownerId;
    }

    private void SetInviteCode(string? inviteCode)
    {
        if (Privacy == PublicPrivacy)
        {
            InviteCode = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(inviteCode))
            throw new ArgumentException("Código de convite é obrigatório para bolões privados.");

        inviteCode = inviteCode.Trim().ToUpperInvariant();

        if (inviteCode.Length is < 6 or > 10)
            throw new ArgumentException("Código de convite inválido.");

        InviteCode = inviteCode;
    }
}