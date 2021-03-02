using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Server.API.Infrastructure.Contexts;
using Application.Server.API.Infrastructure.Seeds;
using Application.Server.API.Models.Blog;
using Application.Shared.DTO.Blog;
using Application.Shared.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Server.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class BlogController : ControllerBase
    {
        private const int DEFAULT_PAGE_SIZE = 10;

        private readonly ILogger<BlogController> _logger;
        private readonly BlogDbContext _blogDbContext;

        public BlogController(ILogger<BlogController> logger, BlogDbContext blogDbContext)
        {
            _blogDbContext = blogDbContext ?? throw new ArgumentNullException($"{nameof(blogDbContext)} can't be null.");
            _logger = logger;
        }

        // Put api/<controller>/1
        [HttpPut("{id}")]
        public async Task<ActionResult<Response<int>>> Put(int Id, PostDto postDto)
        {
            var post = await _blogDbContext.Posts.FindAsync(Id).ConfigureAwait(false);
            if (post is null)
                return NotFound(new Response<int>($"Couldn't find any post with ID {Id}."));

            post.Author = postDto.Author;
            post.Title = postDto.Title;
            post.Body = postDto.Body;
            post.Description = postDto.Description;
            post.Tags = postDto.Tags;
            post.Thumbnail = postDto.Thumbnail;

            _blogDbContext.Posts.Update(post);
            await _blogDbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new Response<int>(post.ID));
        }

        // Post api/<controller>/
        [HttpPost]
        public async Task<ActionResult<Response<int>>> Post(PostDto postDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Post mapped = new Post
            {
                Author = postDto.Author,
                Title = postDto.Title,
                Body = postDto.Body,
                Created = DateTime.Now,
                Description = postDto.Description,
                Tags = postDto.Tags,
                Thumbnail = postDto.Thumbnail
            };
            await _blogDbContext.Posts.AddAsync(mapped);
            await _blogDbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new Response<int>(mapped.ID));
        }

        // Get api/<controller>/1
        [HttpGet("{id}")]
        //[ResponseCache(CacheProfileName = "30SecCache")]
        public async Task<ActionResult<Response<PostResponseDto>>> Get(int Id)
        {
            Post post = await _blogDbContext.Posts.FindAsync(Id);
            if (post is null)
                return NotFound(new Response<PostResponseDto>($"Couldn't find any post with ID {Id}."));

            var postDto = new PostResponseDto
            {
                ID = post.ID,
                Author = post.Author,
                Title = post.Title,
                Body = post.Body,
                Description = post.Description,
                Thumbnail = post.Thumbnail,
                Created = post.Created,
                Tags = post.Tags
            };
            return Ok(new Response<PostResponseDto>(postDto));
        }

        // Delete api/<controller>/1
        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<int>>> DeleteAsync(int Id)
        {
            // first time, do find query And Track the Record for future request's, so ef give it from the cache in next find's.
            var post = await _blogDbContext.Posts
                .FindAsync(Id);
            if (post is null)
                return NotFound(new Response<int>($"Couln't find post with ID {Id}."));
            // do delete and save
            _blogDbContext.Posts.Remove(post);
            await _blogDbContext.SaveChangesAsync().ConfigureAwait(false);
            return Ok(new Response<int>(Id));
        }

        // GET api/<controller>/
        [HttpGet]
        public async Task<ActionResult<PagedResponse<IEnumerable<PostResponseDto>>>> GetAsync([FromQuery] int? pageSize, int page = 1)
        {
            pageSize = pageSize.GetValueOrDefault(DEFAULT_PAGE_SIZE);
            // get paged result from posts table and sort them by newer posts
            IEnumerable<Post> posts = await _blogDbContext.Posts
                .AsNoTracking() // remove unnecessary tracking cost for larg data
                .OrderByDescending(x => x.Created)
                .Skip((page - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .ToListAsync().ConfigureAwait(false);
            // get total posts number
            int totalPosts = await _blogDbContext.Posts.CountAsync().ConfigureAwait(false);
            // map ef query result to a DTO for client
            var postsDto = posts.Select(p => new PostResponseDto
            {
                ID = p.ID,
                Author = p.Author,
                Title = p.Title,
                Body = p.Body,
                Created = p.Created,
                Description = p.Description,
                Tags = p.Tags,
                Thumbnail = p.Thumbnail
            });

            _logger.LogInformation("{count} data sent to client", posts.Count());
            // return the paged response
            return Ok(new PagedResponse<IEnumerable<PostResponseDto>>
                (postsDto, page, pageSize.Value, total: totalPosts));
        }

        // GET api/<controller>/search/text
        [HttpGet("search/{term}")]
        public async Task<ActionResult<Response<IEnumerable<PostResponseDto>>>> Search(string term)
        {
            var posts = await _blogDbContext.Posts.AsNoTracking()
                .Where(x => x.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Created)
                .ToListAsync().ConfigureAwait(false);
            if (!posts.Any())
                return NotFound(new Response<PostResponseDto>($"No Match Found with term {term}."));

            var postsDto = posts.Select(p => new PostResponseDto
            {
                ID = p.ID,
                Author = p.Author,
                Title = p.Title,
                Body = p.Body,
                Created = p.Created,
                Description = p.Description,
                Tags = p.Tags,
                Thumbnail = p.Thumbnail
            });
            return Ok(new Response<IEnumerable<PostResponseDto>>(postsDto));
        }
    }
}
