namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired().HasMaxLength(128);
        b.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.RevokedAt).HasColumnName("revoked_at");

        b.HasIndex(x => x.TokenHash).IsUnique();

        b.HasOne(x => x.User)
         .WithMany(u => u.RefreshTokens)
         .HasForeignKey(x => x.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
