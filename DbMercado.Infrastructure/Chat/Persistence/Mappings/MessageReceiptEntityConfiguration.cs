using DbMercado.Domain.Chat.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Chat.Persistence.Mappings;

public sealed class MessageReceiptEntityConfiguration : IEntityTypeConfiguration<MessageReceiptEntity>
{
    public void Configure(EntityTypeBuilder<MessageReceiptEntity> builder)
    {
        builder.ToTable("MessageReceipts");

        builder.HasKey(x => new { x.MessageId, x.UserId });

        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.DeliveredAt);
        builder.Property(x => x.ReadAt);

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
