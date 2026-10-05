using Microsoft.EntityFrameworkCore;
using Movies.Domain;

namespace Movies.Infrastructure.Persistence;

public sealed class MoviesDbContext(DbContextOptions<MoviesDbContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Actor> Actors => Set<Actor>();

    public DbSet<CastMember> CastMembers => Set<CastMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Trigram indexes (pg_trgm) let "contains" searches like '%batman%' use an index.
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoviesDbContext).Assembly);
    }
}
