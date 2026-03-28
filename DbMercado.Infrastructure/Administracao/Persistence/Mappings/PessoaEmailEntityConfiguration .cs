using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PessoaEmailEntityConfiguration : IEntityTypeConfiguration<PessoaEmailEntity>
{
    public void Configure(EntityTypeBuilder<PessoaEmailEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPessoaEmail");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd();

        builder.Property(p => p.Email)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(p => p.TipoEmail)
               .HasMaxLength(50);

        #endregion




        #region Relacionamentos

        builder.HasOne(p => p.Pessoa)
               .WithMany(p => p.Emails)
               .HasForeignKey(p => p.PessoaId);

        #endregion




        #region Índices

        builder.HasIndex(p => p.Email);

        #endregion
    }
}
