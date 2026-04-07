using DbMercado.Domain.Chat.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Chat.Persistence.Mappings;

public sealed class ChatInviteEntityConfiguration : IEntityTypeConfiguration<ChatInviteEntity>
{
    public void Configure(EntityTypeBuilder<ChatInviteEntity> builder)
    {
        builder.ToTable("ChatInvites");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");

        builder.Property(x => x.InvitedBy).HasMaxLength(450).IsRequired();
        builder.Property(x => x.InvitedUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.InvitedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
