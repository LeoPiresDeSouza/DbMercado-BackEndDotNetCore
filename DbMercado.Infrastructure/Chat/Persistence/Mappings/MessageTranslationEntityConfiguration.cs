using DbMercado.Domain.Chat.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Chat.Persistence.Mappings;

public sealed class MessageTranslationEntityConfiguration : IEntityTypeConfiguration<MessageTranslationEntity>
{
    public void Configure(EntityTypeBuilder<MessageTranslationEntity> builder)
    {
        builder.ToTable("MessageTranslations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");

        builder.Property(x => x.TargetLang).HasMaxLength(32).IsRequired();
        builder.Property(x => x.TranslatedText).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.FromCache).IsRequired();
        builder.Property(x => x.TranslatedAt).IsRequired();

        builder.HasIndex(x => new { x.MessageId, x.TargetLang })
            .IsUnique();
    }
}
