using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Domain;

namespace Movies.Infrastructure.Persistence.Configurations;

internal sealed class CastMemberConfiguration : IEntityTypeConfiguration<CastMember>
{
    public void Configure(EntityTypeBuilder<CastMember> cast)
    {
        cast.HasKey(c => new { c.MovieId, c.ActorId });

        cast.HasOne(c => c.Movie).WithMany(m => m.Cast).HasForeignKey(c => c.MovieId);
        cast.HasOne(c => c.Actor).WithMany(a => a.Roles).HasForeignKey(c => c.ActorId);

        cast.HasIndex(c => c.ActorId);
    }
}
