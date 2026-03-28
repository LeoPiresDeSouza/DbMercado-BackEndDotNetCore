using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class UsuarioEntityConfiguration : IEntityTypeConfiguration<UsuarioEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbUsuario");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd();

        builder.Property(p => p.IdentityUserId)
               .HasMaxLength(450)
               .IsRequired();

        builder.Property(p => p.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        #endregion




        #region Relacionamentos

        builder.HasOne(p => p.Pessoa)
               .WithMany()
               .HasForeignKey(p => p.PessoaId);

        #endregion




        #region Índices

        builder.HasIndex(p => p.IdentityUserId)
               .IsUnique();

        #endregion
    }
}
