using DbMercado.Domain.Administracao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class ModuloEntityConfiguration : IEntityTypeConfiguration<ModuloEntity>
{
    public void Configure(EntityTypeBuilder<ModuloEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbModulo");
        builder.HasKey(p => p.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.NomeNormalizado).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NomeExibicao).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Icone).HasMaxLength(50);
        builder.Property(p => p.OrdemExibicao);
        builder.Property(p => p.Descricao).HasMaxLength(4000).IsRequired();

        #endregion Propriedades




        #region Relacionamentos
        #endregion




        #region Índices

        builder.HasIndex(u => u.NomeNormalizado).IsUnique(true);

        #endregion Índices
    }
}
