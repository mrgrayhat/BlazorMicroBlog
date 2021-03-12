using MicroBlog.Server.API.Models.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Server.Infrastructure.Contexts.Configurations.Blog
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(key => key.ID);
            builder.HasIndex(index => index.ID).IsUnique();
            builder.HasIndex(index => index.Title).IsUnique();

            builder.Property(t => t.Title).HasMaxLength(50).IsUnicode().IsRequired();
            builder.Property(b => b.Body).HasMaxLength(5000).IsUnicode().IsRequired();
            builder.Property(desc => desc.Description).HasMaxLength(2500)
            .IsUnicode().IsRequired(false);
            builder.Property(thumb => thumb.Thumbnail).HasMaxLength(254)
            .IsUnicode().IsRequired(false);
            builder.Property(tag => tag.Tags).HasMaxLength(254)
            .IsUnicode().IsRequired(false);
        }
    }
}