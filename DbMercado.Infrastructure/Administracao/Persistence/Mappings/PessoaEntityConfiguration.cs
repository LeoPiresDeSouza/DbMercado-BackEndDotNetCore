using DbMercado.Domain.Administracao.Entities;
using DbMercado.Domain.Administracao.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PessoaEntityConfiguration : IEntityTypeConfiguration<PessoaEntity>
{
    public void Configure(EntityTypeBuilder<PessoaEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPessoa");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd();

        builder.Property(p => p.TipoPessoa)
               .HasMaxLength(20)
               .HasConversion(
                   v => TipoPessoaPersistencia.ParaArmazenamento(v),
                   v => TipoPessoaPersistencia.DeArmazenamento(v));

        builder.Property(p => p.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        #endregion

        #region Relacionamentos

        builder.HasMany(p => p.Telefones)
               .WithOne(p => p.Pessoa)
               .HasForeignKey(p => p.PessoaId);

        builder.HasMany(p => p.Emails)
               .WithOne(p => p.Pessoa)
               .HasForeignKey(p => p.PessoaId);

        builder.HasMany(p => p.Enderecos)
               .WithOne(p => p.Pessoa)
               .HasForeignKey(p => p.PessoaId);

        builder.HasMany(p => p.RedesSociais)
               .WithOne(p => p.Pessoa)
               .HasForeignKey(p => p.PessoaId);

        #endregion

        #region Índices
        #endregion
    }
}
