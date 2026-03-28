using DbMercado.Domain.Importacao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Importacao.Persistence.Mappings;

public class NotaFiscalEntityConfiguration : IEntityTypeConfiguration<NotaFiscalEntity>
{
    public void Configure(EntityTypeBuilder<NotaFiscalEntity> builder)
    {
        builder.ToTable("impNotaFiscal");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ChaveAcesso).IsRequired().HasMaxLength(44);
        builder.HasIndex(e => e.ChaveAcesso).IsUnique();

        builder.Property(e => e.Numero).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Serie).IsRequired().HasMaxLength(10);
        builder.Property(e => e.CnpjEmitente).IsRequired().HasMaxLength(14);
        builder.Property(e => e.RazaoSocialEmitente).HasMaxLength(200);
        builder.Property(e => e.ValorTotal).HasPrecision(18, 4);

        builder.HasMany(e => e.Itens)
            .WithOne(i => i.NotaFiscal)
            .HasForeignKey(i => i.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
