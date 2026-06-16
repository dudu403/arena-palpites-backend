namespace Bolao.Domain.Entities;

public class Championship
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int ExternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PopularName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Season { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}