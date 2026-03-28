using DbMercado.Domain.Administracao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class ParametroEntityConfiguration : IEntityTypeConfiguration<ParametroEntity>
{
    public void Configure(EntityTypeBuilder<ParametroEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbParametro");
        builder.HasKey(p => p.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Categoria).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Atributo).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Chave).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Valor).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(4000).IsRequired(false);

        #endregion Propriedades




        #region Índices

        builder.HasIndex(u => u.Categoria).IsUnique(false);
        builder.HasIndex(u => u.Atributo).IsUnique(false);
        builder.HasIndex(u => u.Chave).IsUnique(false);
        builder.HasIndex(p => new { p.Categoria, p.Atributo, p.Chave }).IsUnique(false);
        builder.HasIndex(p => new { p.Categoria, p.Atributo }).IsUnique(false);

        #endregion Índices
    }
}
