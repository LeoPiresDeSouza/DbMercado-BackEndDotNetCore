using DbMercado.Domain.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Shared.Persistence.Mappings.Providers;

public class AppLogEntityConfiguration : IEntityTypeConfiguration<AppLogEntity>
{
    public void Configure(EntityTypeBuilder<AppLogEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbAppLog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasMaxLength(256);
        builder.Property(x => x.Level).HasMaxLength(16);
        builder.Property(x => x.Message).HasMaxLength(4000);
        builder.Property(x => x.Exception).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ErrorCode).HasMaxLength(64);
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.UserId).HasMaxLength(256);
        builder.Property(x => x.UserName).HasMaxLength(256);
        builder.Property(x => x.Path).HasMaxLength(2048);
        builder.Property(x => x.Method).HasMaxLength(16);
        builder.Property(x => x.Ip).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        #endregion Propriedades




        #region Índices

        builder.HasIndex(u => u.CreatedAt).IsUnique(false);
        builder.HasIndex(u => u.UserId).IsUnique(false);

        #endregion Índices
    }
}
