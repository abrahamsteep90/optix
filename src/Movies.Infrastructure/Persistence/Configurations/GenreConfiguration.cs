using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Domain;

namespace Movies.Infrastructure.Persistence.Configurations;

internal sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> genre)
    {
        genre.Property(g => g.Name).HasMaxLength(50);
        genre.HasIndex(g => g.Name).IsUnique();
    }
}
