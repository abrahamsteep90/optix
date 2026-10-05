using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Domain;

namespace Movies.Infrastructure.Persistence.Configurations;

internal sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> actor)
    {
        actor.Property(a => a.Name).UseCollation("und-x-icu"); // see MovieConfiguration
        actor.HasIndex(a => a.TmdbPersonId).IsUnique();
        actor.HasIndex(a => a.SearchName).HasMethod("gin").HasOperators("gin_trgm_ops");
    }
}
