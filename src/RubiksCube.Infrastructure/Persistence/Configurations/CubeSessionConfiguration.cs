using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RubiksCube.Domain;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Infrastructure.Persistence.Configurations;

/// <summary>Cube as a facelet string, log as an owned table (same transaction), Version as concurrency token.</summary>
internal sealed class CubeSessionConfiguration : IEntityTypeConfiguration<CubeSession>
{
    public void Configure(EntityTypeBuilder<CubeSession> builder)
    {
        builder.ToTable("CubeSessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).ValueGeneratedNever();

        builder.Property(session => session.Cube)
            .HasConversion(cube => cube.ToFacelets(), facelets => Cube.FromFacelets(facelets))
            .HasColumnName("Facelets")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(session => session.CreatedAtUtc).IsRequired();
        builder.Property(session => session.Version).IsConcurrencyToken();

        builder.Ignore(session => session.EffectiveMoves);
        builder.Ignore(session => session.CanUndo);

        builder.OwnsMany(session => session.Log, log =>
        {
            log.ToTable("RotationLog");
            log.WithOwner().HasForeignKey("SessionId");
            log.Property<Guid>("SessionId");
            log.HasKey("SessionId", nameof(RotationLogEntry.Sequence));
            log.Property(entry => entry.Sequence).ValueGeneratedNever();
            log.Property(entry => entry.Kind).HasConversion<string>().HasMaxLength(16);
            log.Property(entry => entry.Face).HasConversion<string?>().HasMaxLength(8);
            log.Property(entry => entry.Rotation).HasConversion<string?>().HasMaxLength(16);
            log.Property(entry => entry.OccurredAtUtc).IsRequired();
            log.Ignore(entry => entry.Move);
        });

        builder.Navigation(session => session.Log).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
    }
}
