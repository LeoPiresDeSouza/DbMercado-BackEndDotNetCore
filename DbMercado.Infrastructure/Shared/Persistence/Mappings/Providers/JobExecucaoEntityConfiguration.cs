using DbMercado.Domain.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Shared.Persistence.Mappings.Providers;

public sealed class JobExecucaoEntityConfiguration : IEntityTypeConfiguration<JobExecucaoEntity>
{
    public void Configure(EntityTypeBuilder<JobExecucaoEntity> builder)
    {
        builder.ToTable("dbJobExecucao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.FireInstanceId).HasMaxLength(256).IsRequired();
        builder.Property(x => x.JobNome).HasMaxLength(128).IsRequired();
        builder.Property(x => x.JobGrupo).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TriggerNome).HasMaxLength(128).IsRequired();
        builder.Property(x => x.TriggerGrupo).HasMaxLength(64).IsRequired();
        builder.Property(x => x.MensagemErro).HasMaxLength(4000);

        builder.HasIndex(x => x.FireInstanceId).IsUnique();
        builder.HasIndex(x => x.InicioUtc);
        builder.HasIndex(x => new { x.JobNome, x.JobGrupo, x.InicioUtc });
    }
}
