using DbMercado.Domain.Administracao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class FuncionalidadeEntityConfiguration : IEntityTypeConfiguration<FuncionalidadeEntity>
{
    public void Configure(EntityTypeBuilder<FuncionalidadeEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbFuncionalidade");
        builder.HasKey(p => p.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.NomeNormalizado).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NomeExibicao).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Icone).HasMaxLength(50);
        builder.Property(p => p.OrdemExibicao);
        builder.Property(p => p.Descricao).HasMaxLength(4000).IsRequired();

        #endregion Propriedades




        #region Relacionamentos

        builder
            .HasOne(f => f.Modulo)
            .WithMany(m => m.Funcionalidades)
            .HasForeignKey(f => f.ModuloId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion




        #region Índices

        builder.HasIndex(u => u.NomeNormalizado).IsUnique(true);

        #endregion Índices
    }
}
