using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroBlog.Server.API.Models.Blog;
using MicroBlog.Server.Infrastructure.Contexts.Configurations.Blog;
using MicroBlog.Server.Infrastructure.Contexts.Configurations.Identity;
using MicroBlog.Server.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MicroBlog.Server.API.Infrastructure.Contexts
{
    public class BlogDbContext : IdentityDbContext<UserInfo>, IBlogDbContext
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

            #region Identity Users & Roles
            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new UserRoleConfiguration());
            #endregion

            #region Blog
            builder.ApplyConfiguration(new FileConfiguration());
            builder.ApplyConfiguration(new PostConfiguration());
            #endregion
        }
    }
}
