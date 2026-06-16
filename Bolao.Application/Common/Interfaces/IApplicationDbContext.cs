using Bolao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Bolao.Domain.Entities.Bolao> Boloes { get; }
    DbSet<BolaoMember> BolaoMembers { get; }
    DbSet<BolaoRules> BolaoRules { get; }
    DbSet<Championship> Championships { get; }
    DbSet<FootballTeam> FootballTeams { get; }
    DbSet<FootballMatch> FootballMatches { get; }
    DbSet<FootballGroupStanding> FootballGroupStandings { get; }
    DbSet<Prediction> Predictions { get; }
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}