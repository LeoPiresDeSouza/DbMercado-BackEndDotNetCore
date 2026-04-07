using DbMercado.Domain.Chat.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Chat.Persistence.Mappings;

public sealed class MessageEntityConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");

        builder.Property(x => x.SenderId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.ContentOriginal).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.SourceLang).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MessageType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.TranslationStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.SentAt).IsRequired();
        builder.Property(x => x.EditedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne(x => x.Message)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Receipts)
            .WithOne(x => x.Message)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RoomId, x.SentAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Messages_RoomId_SentAt");
    }
}
