using DbMercado.Domain.Produto.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Produto.Persistence.Mappings;

public class CategoriaProdutoEntityConfiguration : IEntityTypeConfiguration<CategoriaProdutoEntity>
{
    public void Configure(EntityTypeBuilder<CategoriaProdutoEntity> builder)
    {
        builder.ToTable("prdCategoria");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(128);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(160);
        builder.Property(c => c.Descricao).HasMaxLength(1000);
        builder.Property(c => c.Nivel).IsRequired();
        builder.Property(c => c.Ativo).IsRequired();
        builder.Property(c => c.CategoriaPaiId).IsRequired(false);

        builder.HasOne(c => c.CategoriaPai)
            .WithMany(c => c.Subcategorias)
            .HasForeignKey(c => c.CategoriaPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.Nivel);
        builder.HasIndex(c => c.CategoriaPaiId);
        builder.HasIndex(c => new { c.CategoriaPaiId, c.Ativo });
    }
}
