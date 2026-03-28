using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DbMercado.Domain.Administracao.Entities;

namespace DbMercado.Infrastructure.Administracao.Persistence.Mappings;

public class PessoaEnderecoEntityConfiguration : IEntityTypeConfiguration<PessoaEnderecoEntity>
{
    public void Configure(EntityTypeBuilder<PessoaEnderecoEntity> builder)
    {
        #region Propriedades

        builder.ToTable("dbPessoaEndereco");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
               .ValueGeneratedOnAdd();

        builder.Property(p => p.TipoLogradouro)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.Logradouro)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Numero)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(p => p.Complemento)
               .HasMaxLength(200);

        builder.Property(p => p.Bairro)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Cidade)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Estado)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.Pais)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(p => p.Cep)
               .HasMaxLength(20)
               .IsRequired();

        #endregion




        #region Relacionamentos

        builder.HasOne(p => p.Pessoa)
               .WithMany(p => p.Enderecos)
               .HasForeignKey(p => p.PessoaId);

        #endregion




        #region Índices
        #endregion
    }
}
