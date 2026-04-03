using DbMercado.Domain.Produto.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Produto.Persistence.Mappings;

public class ProdutoEntityConfiguration : IEntityTypeConfiguration<ProdutoEntity>
{
    public void Configure(EntityTypeBuilder<ProdutoEntity> builder)
    {
        builder.ToTable("prdProduto");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(256);
        builder.Property(p => p.Descricao).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.Marca).HasMaxLength(128);
        builder.Property(p => p.Modelo).HasMaxLength(128);
        builder.Property(p => p.Gtin).HasMaxLength(32);
        builder.Property(p => p.CategoriaProdutoId).IsRequired(false);
        builder.Property(p => p.UnidadeComercializacao).IsRequired().HasMaxLength(16);
        builder.Property(p => p.UnidadeMedidaFisica).IsRequired().HasMaxLength(16);
        builder.Property(p => p.TipoEmbalagem).IsRequired().HasMaxLength(16);

        builder.OwnsOne(p => p.DimensaoProduto, d =>
        {
            d.Property(x => x.Altura).HasPrecision(18, 4);
            d.Property(x => x.Largura).HasPrecision(18, 4);
            d.Property(x => x.Comprimento).HasPrecision(18, 4);
            d.Property(x => x.Peso).HasPrecision(18, 4);
            d.Property(x => x.UnidadeDimensao).IsRequired().HasMaxLength(8);
            d.Property(x => x.UnidadePeso).IsRequired().HasMaxLength(8);
        });

        builder.Navigation(p => p.DimensaoProduto).IsRequired(false);

        builder.OwnsOne(p => p.DimensaoEmbalagem, d =>
        {
            d.Property(x => x.Altura).HasPrecision(18, 4);
            d.Property(x => x.Largura).HasPrecision(18, 4);
            d.Property(x => x.Comprimento).HasPrecision(18, 4);
            d.Property(x => x.Peso).HasPrecision(18, 4);
            d.Property(x => x.UnidadeDimensao).IsRequired().HasMaxLength(8);
            d.Property(x => x.UnidadePeso).IsRequired().HasMaxLength(8);
        });

        builder.OwnsOne(p => p.OrigemProduto, o =>
        {
            o.Property(x => x.Tipo).IsRequired().HasMaxLength(32);
            o.Property(x => x.PaisOrigem).HasMaxLength(128);
        });

        builder.OwnsOne(p => p.DadosFiscais, f =>
        {
            f.Property(x => x.Ncm).IsRequired().HasMaxLength(8);
            f.Property(x => x.Cest).HasMaxLength(7);
            f.Property(x => x.Origem).IsRequired().HasMaxLength(8);
        });

        builder.OwnsMany(p => p.Atributos, a =>
        {
            a.ToTable("prdProdutoAtributo");
            a.WithOwner().HasForeignKey("ProdutoId");
            a.Property<int>("Id").ValueGeneratedOnAdd();
            a.HasKey("Id");
            a.Property(x => x.Nome).IsRequired().HasMaxLength(128);
            a.Property(x => x.Valor).IsRequired().HasMaxLength(1024);
        });

        builder.HasMany(p => p.Skus)
            .WithOne(s => s.Produto)
            .HasForeignKey(s => s.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CategoriaProduto)
            .WithMany()
            .HasForeignKey(p => p.CategoriaProdutoId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
