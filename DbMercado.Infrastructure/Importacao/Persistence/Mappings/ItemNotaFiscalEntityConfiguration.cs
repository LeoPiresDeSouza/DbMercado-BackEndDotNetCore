using DbMercado.Domain.Importacao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Importacao.Persistence.Mappings;

public class ItemNotaFiscalEntityConfiguration : IEntityTypeConfiguration<ItemNotaFiscalEntity>
{
    public void Configure(EntityTypeBuilder<ItemNotaFiscalEntity> builder)
    {
        builder.ToTable("impItemNotaFiscal");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CodigoProdutoFornecedor).HasMaxLength(60);
        builder.Property(e => e.Descricao).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Ncm).HasMaxLength(8);
        builder.Property(e => e.Quantidade).HasPrecision(18, 4);
        builder.Property(e => e.ValorUnitario).HasPrecision(18, 4);
        builder.Property(e => e.ValorTotalLinha).HasPrecision(18, 4);

        builder.HasIndex(e => new { e.NotaFiscalId, e.NumeroItem }).IsUnique();
    }
}
