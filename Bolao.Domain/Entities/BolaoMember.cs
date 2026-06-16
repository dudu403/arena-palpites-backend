namespace Bolao.Domain.Entities;

public class BolaoMember
{
    public const string OwnerRole = "Owner";
    public const string AdminRole = "Admin";
    public const string MemberRole = "Member";

    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid BolaoId { get; private set; }
    public Bolao Bolao { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string Role { get; private set; } = string.Empty;

    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

    private BolaoMember() { }

    public BolaoMember(Guid bolaoId, Guid userId, string role)
    {
        if (bolaoId == Guid.Empty)
            throw new ArgumentException("Bolão inválido.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Usuário inválido.");

        if (!IsValidRole(role))
            throw new ArgumentException("Perfil do membro inválido.");

        BolaoId = bolaoId;
        UserId = userId;
        Role = role.Trim();
    }

    private static bool IsValidRole(string role)
    {
        return role == OwnerRole ||
               role == AdminRole ||
               role == MemberRole;
    }
}