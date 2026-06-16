namespace Bolao.Domain.Entities;

public class FootballTeam
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Acronym { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}