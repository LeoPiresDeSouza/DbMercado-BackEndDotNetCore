using DbMercado.Domain.Importacao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Importacao.Persistence.Mappings;

public class ProdutoImportadoEntityConfiguration : IEntityTypeConfiguration<ProdutoImportadoEntity>
{
    public void Configure(EntityTypeBuilder<ProdutoImportadoEntity> builder)
    {
        builder.ToTable("impProdutoImportado");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CodigoInterno).IsRequired().HasMaxLength(64);
        builder.HasIndex(e => e.CodigoInterno).IsUnique();

        builder.Property(e => e.Descricao).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Ncm).HasMaxLength(8);
        builder.Property(e => e.UnidadeMedida).HasMaxLength(16);

        builder.HasOne(e => e.ItemNotaFiscalOrigem)
            .WithMany()
            .HasForeignKey(e => e.ItemNotaFiscalOrigemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
