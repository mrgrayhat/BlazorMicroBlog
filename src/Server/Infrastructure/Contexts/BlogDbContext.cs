using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroBlog.Server.API.Models.Blog;
using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Server.API.Infrastructure.Contexts
{
    public class BlogDbContext : DbContext, IBlogDbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        public DbSet<File> Files { get; set; }
        public DbSet<Post> Posts { get; set; }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //All Decimals will have 18,6 Range
            foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,6)");
            }

            base.OnModelCreating(builder);


            builder.Entity<File>((opt) =>
            {
                opt.HasKey(key => key.ID);
                opt.HasIndex(idx => idx.Name).IsUnique();

                opt.Property(u => u.Creator).HasMaxLength(25).IsUnicode().IsRequired();
                opt.Property(n => n.Name).HasMaxLength(50).IsUnicode().IsRequired();
                opt.Property(desc => desc.Description).HasMaxLength(500)
                .IsUnicode().IsRequired(false);
                opt.Property(s => s.Size).IsRequired();
                opt.Property(c => c.ContentType).HasMaxLength(20)
                .IsUnicode().IsRequired();
            });

            builder.Entity<Post>((opt) =>
            {
                opt.HasKey(key => key.ID);
                opt.HasIndex(idx => idx.ID).IsUnique();

                opt.Property(a => a.Author).HasMaxLength(25).IsUnicode().IsRequired();
                opt.Property(t => t.Title).HasMaxLength(50).IsUnicode().IsRequired();
                opt.Property(b => b.Body).HasMaxLength(5000).IsUnicode().IsRequired();
                opt.Property(desc => desc.Description).HasMaxLength(2500)
                .IsUnicode().IsRequired(false);
                opt.Property(thumb => thumb.Thumbnail).HasMaxLength(254)
                .IsUnicode().IsRequired(false);
                opt.Property(tag => tag.Tags).HasMaxLength(254)
                .IsUnicode().IsRequired(false);
            });
        }
    }
}
