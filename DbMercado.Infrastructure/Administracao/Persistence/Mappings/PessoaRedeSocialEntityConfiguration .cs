using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PessoaRedeSocialEntityConfiguration : IEntityTypeConfiguration<PessoaRedeSocialEntity>
{
    public void Configure(EntityTypeBuilder<PessoaRedeSocialEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPessoaRedeSocial");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd();

        builder.Property(p => p.TipoRedeSocial)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.Perfil)
               .HasMaxLength(400)
               .IsRequired();

        #endregion




        #region Relacionamentos

        builder.HasOne(p => p.Pessoa)
               .WithMany(p => p.RedesSociais)
               .HasForeignKey(p => p.PessoaId);

        #endregion




        #region Índices
        #endregion
    }
}
