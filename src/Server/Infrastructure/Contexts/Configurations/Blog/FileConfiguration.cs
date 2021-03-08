using MicroBlog.Server.API.Models.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Server.Infrastructure.Contexts.Configurations.Blog
{
    public class FileConfiguration : IEntityTypeConfiguration<File>
    {
        public void Configure(EntityTypeBuilder<File> builder)
        {
            builder.HasKey(key => key.ID);
            builder.HasIndex(idx => idx.Name).IsUnique();

            builder.Property(u => u.Creator).HasMaxLength(25).IsUnicode().IsRequired();
            builder.Property(n => n.Name).HasMaxLength(50).IsUnicode().IsRequired();
            builder.Property(desc => desc.Description).HasMaxLength(500).IsUnicode().IsRequired(false);
            builder.Property(s => s.Size).IsRequired();
            builder.Property(c => c.ContentType).HasMaxLength(20).IsRequired();
        }
    }
}
