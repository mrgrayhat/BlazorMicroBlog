using System.Threading;
using System.Threading.Tasks;
using MicroBlog.Server.API.Models.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MicroBlog.Server.API.Infrastructure.Contexts
{
    public interface IBlogDbContext
    {
        DbSet<Post> Posts { get; set; }
        DbSet<File> Files { get; set; }
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToke = default);
    }
}
