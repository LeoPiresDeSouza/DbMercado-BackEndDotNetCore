using DbMercado.Domain.Administracao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PermissaoEntityConfiguration : IEntityTypeConfiguration<PermissaoEntity>
{
    public void Configure(EntityTypeBuilder<PermissaoEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPermissao");
        builder.HasKey(p => p.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Permissao).HasMaxLength(200).IsRequired();

        #endregion Propriedades




        #region Relacionamentos

        builder
            .HasOne(f => f.Funcionalidade)
            .WithMany( m => m.Permissoes )
            .HasForeignKey(f => f.FuncionalidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion




        #region Índices

        builder.HasIndex(u => u.Permissao).IsUnique(false);

        #endregion Índices
    }
}
