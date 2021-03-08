using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MicroBlog.Server.API.Infrastructure.Contexts;
using MicroBlog.Server.API.Models.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MicroBlog.Server.API.Infrastructure.Seeds
{
    public interface IDbInitializerService
    {
        /// /// <summary>
        /// Applies any pending migrations for the context to the database.
        /// Will create the database if it does not already exist.
        /// </summary>
        /// <param name="isTest">Recreate database if true, othewise only check/apply migrations</param>
        void Initialize(bool isTest = false);

        /// <summary>
        /// Adds some demo data to the Db
        /// </summary>
        /// <param name="addExtra">will generate more posts if true, otherwise just add default post</param>
        /// <returns></returns>
        Task SeedData(bool addExtra = true);

        /// <summary>
        /// add more demo data
        /// </summary>
        /// <param name="max">max demo data to generate </param>
        /// <returns><see cref="Task.Status"/></returns>
        Task SeedExtraPosts(int max = 10);
    }

    public class DbInitializerService : IDbInitializerService
    {

        private static readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;

        public DbInitializerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        /// <summary>
        /// run once in program life
        /// </summary>
        static DbInitializerService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// db context initial and migration check
        /// </summary>
        /// <param name="isTest">Recreate database if true, othewise only check/apply migrations</param>
        public void Initialize(bool isTest = false)
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<BlogDbContext>();
            if (isTest)
            {
                context.Database.EnsureCreated();
            }
            else
            {
                if (context.Database.IsSqlServer())
                {
                    context.Database.Migrate();
                }
            }

        }

        /// <summary>
        /// seed some demo data to the blog
        /// </summary>
        /// <param name="addExtra">will generate more posts if true, otherwise just add default post</param>
        /// <returns><see cref="Task.Status"/></returns>
        public async Task SeedData(bool addExtra = true)
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<BlogDbContext>())
                {
                    if (!context.Posts.Any())
                    {
                        Post helloWorldPost = new Post
                        {
                            Author = "admin",
                            Title = "Hello World",
                            Body = "Hooray, First post in this blog!",
                            Description = "this is just a simple post",
                            Tags = "HelloWorld;FirstPost;MicroBlog",
                            Created = DateTime.Now,
                            Thumbnail = await GetRandomThumbnail(null).ConfigureAwait(false)
                        };
                        await context.AddAsync(helloWorldPost);
                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
            }
            // if extra was true, add more posts to db
            if (addExtra)
                await SeedExtraPosts().ConfigureAwait(false);
        }

        /// <summary>
        /// add more demo data
        /// </summary>
        /// <param name="max">max demo data to generate </param>
        /// <returns><see cref="Task.Status"/></returns>
        public async Task SeedExtraPosts(int max = 10)
        {
            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            using BlogDbContext context = serviceScope.ServiceProvider.GetService<BlogDbContext>();

            var posts = new List<Post>(max);
            for (int i = 1; i < max; i++)
            {
                posts.Add(new Post
                {
                    Author = "admin",
                    Title = $"Post {i}",
                    Body = $"This is post {i} contents.",
                    Description = $"Post {i} description",
                    Tags = $"Post{i};tag{i}",
                    Created = DateTime.Now,
                    Thumbnail = await GetRandomThumbnail(null).ConfigureAwait(false)
                });
            }

            await context.AddRangeAsync(posts);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// get a random picture url from an online site
        /// </summary>
        /// <param name="randomKey">a number to avoid duplicate results</param>
        /// <returns>an image url that poin to it's online address</returns>
        public static async Task<string> GetRandomThumbnail(int? randomKey)
        {
            Log.Information("Getting Online Random Thumbnail for seeding posts...");
            randomKey = randomKey.GetValueOrDefault(new Random().Next());
            using HttpResponseMessage response = await _httpClient.GetAsync($"https://picsum.photos/200/200/?random={randomKey.Value}").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.RequestMessage.RequestUri.ToString();
        }

    }
}
