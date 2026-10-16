namespace ChatApp.Infrastructure.Persistence.Configurations;

using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> b)
    {
        b.ToTable("recommendations", "rec");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.TargetUserId).HasColumnName("target_user_id");
        b.Property(x => x.Score).HasColumnName("score");
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(256);
        b.Property(x => x.CreatedAt).HasColumnName("created_at");

        b.HasIndex(x => new { x.UserId, x.Score });
        b.HasIndex(x => new { x.UserId, x.TargetUserId }).IsUnique();
    }
}
