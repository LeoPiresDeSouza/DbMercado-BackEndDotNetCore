using DbMercado.Domain.Chat.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DbMercado.Infrastructure.Chat.Persistence.Mappings;

public sealed class ChatMemberEntityConfiguration : IEntityTypeConfiguration<ChatMemberEntity>
{
    public void Configure(EntityTypeBuilder<ChatMemberEntity> builder)
    {
        builder.ToTable("ChatMembers");

        builder.HasKey(x => new { x.RoomId, x.UserId });

        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.LanguagePref).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
