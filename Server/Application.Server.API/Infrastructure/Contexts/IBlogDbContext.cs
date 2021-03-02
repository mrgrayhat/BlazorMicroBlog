using System.Threading;
using System.Threading.Tasks;
using Application.Server.API.Models.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Server.API.Infrastructure.Contexts
{
    public interface IBlogDbContext
    {
        DbSet<Post> Posts { get; set; }
        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToke = default);
    }
}
