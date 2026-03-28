using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PessoaJuridicaEntityConfiguration : IEntityTypeConfiguration<PessoaJuridicaEntity>
{
    public void Configure(EntityTypeBuilder<PessoaJuridicaEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPessoaJuridica");

        builder.HasKey(p => p.PessoaId);

        builder.Property(p => p.RazaoSocial)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(p => p.NomeFantasia)
               .HasMaxLength(300);

        builder.Property(p => p.Cnpj)
               .HasMaxLength(18);

        #endregion




        #region Relacionamentos

        builder.HasOne(p => p.Pessoa)
               .WithOne(pe => pe.Juridica)
               .HasForeignKey<PessoaJuridicaEntity>(p => p.PessoaId);

        #endregion




        #region Índices

        builder.HasIndex(p => p.Cnpj)
               .IsUnique();

        #endregion
    }
}
