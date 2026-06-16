namespace Bolao.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FirebaseUid { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhotoUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    private User() { }
    public User(string firebaseUid, string name, string email, string? photoUrl)
    {
        FirebaseUid = firebaseUid.Trim();
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
    }

    public void UpdateProfile(string name, string? photoUrl)
    {
        if (!IsActive)
            throw new InvalidOperationException("Usuário inativo não pode ser atualizado.");

        Name = name.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}