using Bolao.Application.Common.Interfaces;
using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Bolao.Domain.Entities.Bolao> Boloes => Set<Bolao.Domain.Entities.Bolao>();

    public DbSet<BolaoMember> BolaoMembers => Set<BolaoMember>();

    public DbSet<BolaoRules> BolaoRules => Set<BolaoRules>();

    public DbSet<Championship> Championships => Set<Championship>();

    public DbSet<FootballTeam> FootballTeams => Set<FootballTeam>();

    public DbSet<FootballMatch> FootballMatches => Set<FootballMatch>();

    public DbSet<FootballGroupStanding> FootballGroupStandings => Set<FootballGroupStanding>();

    public DbSet<Prediction> Predictions => Set<Prediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureBolao(modelBuilder);
        ConfigureBolaoMember(modelBuilder);
        ConfigureBolaoRules(modelBuilder);

        ConfigureChampionship(modelBuilder);
        ConfigureFootballTeam(modelBuilder);
        ConfigureFootballMatch(modelBuilder);
        ConfigureFootballGroupStanding(modelBuilder);

        ConfigurePrediction(modelBuilder);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.ToTable("Users");
        entity.HasKey(u => u.Id);

        entity.Property(u => u.FirebaseUid).IsRequired().HasMaxLength(100);
        entity.HasIndex(u => u.FirebaseUid).IsUnique();

        entity.Property(u => u.Name).IsRequired().HasMaxLength(150);
        entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
        entity.HasIndex(u => u.Email);

        entity.Property(u => u.PhotoUrl).HasMaxLength(500);
        entity.Property(u => u.IsActive).IsRequired();
        entity.Property(u => u.CreatedAt).IsRequired();
        entity.Property(u => u.UpdatedAt);
    }

    private void ConfigureBolao(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Bolao.Domain.Entities.Bolao>();

        entity.ToTable("Boloes");
        entity.HasKey(b => b.Id);

        entity.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(80);

        entity.Property(b => b.Championship)
            .IsRequired()
            .HasMaxLength(80);

        entity.Property(b => b.ChampionshipExternalId)
            .IsRequired();

        entity.HasIndex(b => b.ChampionshipExternalId);

        entity.Property(b => b.Description)
            .HasMaxLength(200);

        entity.Property(b => b.MaxParticipants)
            .IsRequired();

        entity.Property(b => b.Privacy)
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(b => b.InviteCode)
            .HasMaxLength(10);

        entity.HasIndex(b => b.InviteCode)
            .IsUnique()
            .HasFilter("[InviteCode] IS NOT NULL");

        entity.Property(b => b.CreatedAt)
            .IsRequired();

        entity.Property(b => b.UpdatedAt);

        entity.HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureBolaoMember(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<BolaoMember>();

        entity.ToTable("BolaoMembers");
        entity.HasKey(m => m.Id);

        entity.Property(m => m.Role)
            .IsRequired()
            .HasMaxLength(30);

        entity.Property(m => m.JoinedAt)
            .IsRequired();

        entity.HasOne(m => m.Bolao)
            .WithMany()
            .HasForeignKey(m => m.BolaoId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(m => new { m.BolaoId, m.UserId })
            .IsUnique();

        entity.HasIndex(m => m.UserId);
    }

    private void ConfigureBolaoRules(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<BolaoRules>();

        entity.ToTable("BolaoRules");
        entity.HasKey(r => r.Id);

        entity.Property(r => r.ExactScorePoints).IsRequired();
        entity.Property(r => r.WinnerPoints).IsRequired();
        entity.Property(r => r.DrawPoints).IsRequired();

        entity.HasOne(r => r.Bolao)
            .WithOne(b => b.Rules)
            .HasForeignKey<BolaoRules>(r => r.BolaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureChampionship(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Championship>();

        entity.ToTable("Championships");
        entity.HasKey(c => c.Id);

        entity.Property(c => c.ExternalId)
            .IsRequired();

        entity.HasIndex(c => c.ExternalId)
            .IsUnique();

        entity.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(120);

        entity.Property(c => c.PopularName)
            .HasMaxLength(120);

        entity.Property(c => c.Slug)
            .HasMaxLength(150);

        entity.Property(c => c.Season)
            .HasMaxLength(20);

        entity.Property(c => c.Status)
            .HasMaxLength(40);

        entity.Property(c => c.Type)
            .HasMaxLength(40);

        entity.Property(c => c.Region)
            .HasMaxLength(80);

        entity.Property(c => c.LogoUrl)
            .HasMaxLength(600);

        entity.Property(c => c.CreatedAt)
            .IsRequired();

        entity.Property(c => c.UpdatedAt);
    }

    private void ConfigureFootballTeam(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FootballTeam>();

        entity.ToTable("FootballTeams");
        entity.HasKey(t => t.Id);

        entity.Property(t => t.ExternalId)
            .IsRequired();

        entity.HasIndex(t => t.ExternalId)
            .IsUnique();

        entity.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(120);

        entity.Property(t => t.Acronym)
            .HasMaxLength(10);

        entity.Property(t => t.LogoUrl)
            .HasMaxLength(600);

        entity.Property(t => t.CreatedAt)
            .IsRequired();

        entity.Property(t => t.UpdatedAt);
    }

    private void ConfigureFootballMatch(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FootballMatch>();

        entity.ToTable("FootballMatches");
        entity.HasKey(m => m.Id);

        entity.Property(m => m.ExternalId)
            .IsRequired();

        entity.HasIndex(m => m.ExternalId)
            .IsUnique();

        entity.Property(m => m.ChampionshipExternalId)
            .IsRequired();

        entity.Property(m => m.PhaseExternalId)
            .IsRequired(false);

        entity.Property(m => m.GroupName)
            .HasMaxLength(80);

        entity.Property(m => m.GroupSlug)
            .HasMaxLength(80);

        entity.Property(m => m.RoundName)
            .HasMaxLength(80);

        entity.Property(m => m.RoundSlug)
            .HasMaxLength(80);

        entity.Property(m => m.RoundNumber)
            .IsRequired(false);

        entity.Property(m => m.ScoreText)
            .HasMaxLength(150);

        entity.Property(m => m.Status)
            .IsRequired()
            .HasMaxLength(40);

        entity.Property(m => m.Slug)
            .HasMaxLength(180);

        entity.Property(m => m.HomeTeamExternalId)
            .IsRequired();

        entity.Property(m => m.AwayTeamExternalId)
            .IsRequired();

        entity.Property(m => m.HomeScore)
            .IsRequired(false);

        entity.Property(m => m.AwayScore)
            .IsRequired(false);

        entity.Property(m => m.HasPenaltyShootout)
            .IsRequired();

        entity.Property(m => m.MatchDate)
            .IsRequired(false);

        entity.Property(m => m.MatchDateText)
            .HasMaxLength(20);

        entity.Property(m => m.MatchTimeText)
            .HasMaxLength(10);

        entity.Property(m => m.StadiumExternalId)
            .IsRequired(false);

        entity.Property(m => m.StadiumName)
            .HasMaxLength(120);

        entity.Property(m => m.PointsCalculated)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(m => m.CreatedAt)
            .IsRequired();

        entity.Property(m => m.UpdatedAt);

        entity.HasIndex(m => m.ChampionshipExternalId);
        entity.HasIndex(m => m.HomeTeamExternalId);
        entity.HasIndex(m => m.AwayTeamExternalId);
        entity.HasIndex(m => m.MatchDate);
        entity.HasIndex(m => new { m.ChampionshipExternalId, m.RoundNumber });

        entity.HasIndex(m => new
        {
            m.Status,
            m.PointsCalculated
        });
    }

    private void ConfigureFootballGroupStanding(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FootballGroupStanding>();

        entity.ToTable("FootballGroupStandings");
        entity.HasKey(s => s.Id);

        entity.Property(s => s.ChampionshipExternalId)
            .IsRequired();

        entity.Property(s => s.PhaseExternalId)
            .IsRequired(false);

        entity.Property(s => s.GroupName)
            .IsRequired()
            .HasMaxLength(80);

        entity.Property(s => s.GroupSlug)
            .HasMaxLength(80);

        entity.Property(s => s.TeamExternalId)
            .IsRequired();

        entity.Property(s => s.Position)
            .IsRequired();

        entity.Property(s => s.Points)
            .IsRequired();

        entity.Property(s => s.Games)
            .IsRequired();

        entity.Property(s => s.Wins)
            .IsRequired();

        entity.Property(s => s.Draws)
            .IsRequired();

        entity.Property(s => s.Losses)
            .IsRequired();

        entity.Property(s => s.GoalsFor)
            .IsRequired();

        entity.Property(s => s.GoalsAgainst)
            .IsRequired();

        entity.Property(s => s.GoalDifference)
            .IsRequired();

        entity.Property(s => s.Performance)
            .HasPrecision(5, 2)
            .IsRequired();

        entity.Property(s => s.PositionVariation)
            .IsRequired();

        entity.Property(s => s.QualificationZone)
            .HasMaxLength(80);

        entity.Property(s => s.CreatedAt)
            .IsRequired();

        entity.Property(s => s.UpdatedAt);

        entity.HasIndex(s => new
        {
            s.ChampionshipExternalId,
            s.PhaseExternalId,
            s.GroupSlug,
            s.TeamExternalId
        }).IsUnique();

        entity.HasIndex(s => s.TeamExternalId);
    }

    private void ConfigurePrediction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Prediction>();

        entity.ToTable("Predictions");
        entity.HasKey(p => p.Id);

        entity.Property(p => p.HomeScore)
            .IsRequired();

        entity.Property(p => p.AwayScore)
            .IsRequired();

        entity.Property(p => p.PointsEarned)
            .IsRequired()
            .HasDefaultValue(0);

        entity.Property(p => p.ExactScoreHit)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(p => p.WinnerHit)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(p => p.CreatedAt)
            .IsRequired();

        entity.Property(p => p.UpdatedAt);

        entity.HasIndex(p => new
        {
            p.BolaoId,
            p.UserId,
            p.FootballMatchId
        }).IsUnique();

        entity.HasIndex(p => p.UserId);
        entity.HasIndex(p => p.BolaoId);
        entity.HasIndex(p => p.FootballMatchId);

        entity.HasOne(p => p.Bolao)
            .WithMany()
            .HasForeignKey(p => p.BolaoId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(p => p.FootballMatch)
            .WithMany()
            .HasForeignKey(p => p.FootballMatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}