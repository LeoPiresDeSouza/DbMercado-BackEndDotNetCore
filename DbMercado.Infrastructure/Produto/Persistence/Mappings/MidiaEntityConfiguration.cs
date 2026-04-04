using DbMercado.Domain.Produto.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Produto.Persistence.Mappings;

public class MidiaEntityConfiguration : IEntityTypeConfiguration<MidiaEntity>
{
    public void Configure(EntityTypeBuilder<MidiaEntity> builder)
    {
        builder.ToTable("prdMidia");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.Url).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.ThumbnailUrl).HasMaxLength(2000);
        builder.Property(m => m.Tipo).IsRequired().HasMaxLength(8);
        builder.Property(m => m.Ordem).IsRequired();
        builder.Property(m => m.IsPrincipal).IsRequired();
        builder.Property(m => m.Duracao).HasPrecision(10, 2);
        builder.Property(m => m.Status).IsRequired().HasMaxLength(16);

        builder.HasOne(m => m.Produto)
            .WithMany()
            .HasForeignKey(m => m.ProdutoId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Ignore(m => m.RowVersion);
    }
}
