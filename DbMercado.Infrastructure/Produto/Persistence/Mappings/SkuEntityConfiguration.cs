using DbMercado.Domain.Produto.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Produto.Persistence.Mappings;

public class SkuEntityConfiguration : IEntityTypeConfiguration<SkuEntity>
{
    public void Configure(EntityTypeBuilder<SkuEntity> builder)
    {
        builder.ToTable("prdSku");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.Codigo).IsRequired().HasMaxLength(64);
        builder.Property(s => s.Ativo).IsRequired();

        builder.HasIndex(s => new { s.ProdutoId, s.Codigo }).IsUnique();
    }
}
