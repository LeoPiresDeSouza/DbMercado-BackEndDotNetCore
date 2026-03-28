using DbMercado.Domain.Administracao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PessoaFisicaEntityConfiguration : IEntityTypeConfiguration<PessoaFisicaEntity>
{
    public void Configure(EntityTypeBuilder<PessoaFisicaEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPessoaFisica");

        builder.HasKey(p => p.PessoaId);

        builder.Property(p => p.Nome)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Sobrenome)
               .HasMaxLength(200);

        builder.Property(p => p.TipoDocumento)
               .HasMaxLength(50);

        builder.Property(p => p.Documento)
               .HasMaxLength(50);

        builder.Property(p => p.Sexo)
               .HasMaxLength(20);

        builder.Property(p => p.DataNascimento);

        #endregion




        #region Relacionamentos

        builder.HasOne(p => p.Pessoa)
               .WithOne(pe => pe.Fisica)
               .HasForeignKey<PessoaFisicaEntity>(p => p.PessoaId);

        #endregion




        #region Índices

        builder.HasIndex(p => p.Documento).IsUnique();

        #endregion
    }
}
