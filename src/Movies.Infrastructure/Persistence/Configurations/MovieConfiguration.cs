using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Domain;

namespace Movies.Infrastructure.Persistence.Configurations;

internal sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> movie)
    {
        // ICU collation sorts titles the way people expect ("a bug's life" before "Zorro", "Æon Flux" with
        // the A's) on every Postgres image. The Alpine image's default collation sorts by raw bytes instead.
        movie.Property(m => m.Title).HasMaxLength(300).UseCollation("und-x-icu");
        movie.Property(m => m.SearchTitle).HasMaxLength(300);
        movie.Property(m => m.OriginalLanguage).HasMaxLength(10);
        movie.Property(m => m.PosterUrl).HasMaxLength(500);
        movie.Property(m => m.VoteAverage).HasPrecision(3, 1);

        movie.HasIndex(m => m.SearchTitle).HasMethod("gin").HasOperators("gin_trgm_ops");
        movie.HasIndex(m => m.Title);
        movie.HasIndex(m => m.ReleaseDate);
        movie.HasIndex(m => m.Popularity);

        movie.HasMany(m => m.Genres)
            .WithMany(g => g.Movies)
            .UsingEntity(join => join.ToTable("movie_genres"));
    }
}
