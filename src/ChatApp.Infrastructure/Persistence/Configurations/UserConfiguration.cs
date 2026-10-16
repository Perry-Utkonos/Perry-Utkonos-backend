namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Username).HasColumnName("username").IsRequired().HasMaxLength(32);
        b.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        b.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(32);
        b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        b.Property(x => x.LastSeenAt).HasColumnName("last_seen_at");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(x => x.Username).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();

        b.HasOne(x => x.Profile)
         .WithOne(p => p.User)
         .HasForeignKey<UserProfile>(p => p.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> b)
    {
        b.ToTable("user_profile");
        // Shared PK: PK = FK = user_id — связь 1:1 с users
        b.HasKey(x => x.UserId);

        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.Bio).HasColumnName("bio").HasMaxLength(500);
        b.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
        b.Property(x => x.BirthDate).HasColumnName("birth_date");
        b.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(16);
        b.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
    }
}
