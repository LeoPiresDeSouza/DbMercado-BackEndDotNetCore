using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PessoaTelefoneEntityConfiguration : IEntityTypeConfiguration<PessoaTelefoneEntity>
{
    public void Configure(EntityTypeBuilder<PessoaTelefoneEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPessoaTelefone");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd();

        builder.Property(p => p.TipoTelefone)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.Ddd)
               .HasMaxLength(5)
               .IsRequired();

        builder.Property(p => p.Numero)
               .HasMaxLength(20)
               .IsRequired();

        #endregion




        #region Relacionamentos

        builder.HasOne(p => p.Pessoa)
               .WithMany(p => p.Telefones)
               .HasForeignKey(p => p.PessoaId);

        #endregion




        #region Índices
        #endregion
    }
}
