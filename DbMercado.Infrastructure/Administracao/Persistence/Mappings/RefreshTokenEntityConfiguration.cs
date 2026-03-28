using DbMercado.Domain.Administracao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("dbRefreshToken");

        builder.HasKey(e => e.Token);

        builder.Property(e => e.Token)
            .HasMaxLength(500);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.ReplacedByToken)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedIpAddress)
            .HasMaxLength(64);

        builder.Property(e => e.RevokedIpAddress)
            .HasMaxLength(64);

        builder.HasIndex(e => e.UserId);
        builder.Ignore(e => e.Usuario);
    }
}
