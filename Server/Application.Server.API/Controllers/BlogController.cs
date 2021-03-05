using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Server.API.Infrastructure.Contexts;
using Application.Server.API.Models.Blog;
using Application.Server.DTOs.Blog;
using Application.Server.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // GET api/<controller>/
        /// <summary>
        /// get blog posts with paging reponse
        /// </summary>
        /// <param name="pageSize">total item per page</param>
        /// <param name="page">page to fetch</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<IEnumerable<PostResponseDto>>>> Index([FromQuery] int? pageSize, int page = 1)
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
                Modified = p.Modified,
                Description = p.Description,
                Tags = p.Tags,
                Thumbnail = p.Thumbnail
            });

            _logger.LogInformation("{count} data sent to client", posts.Count());
            // return the paged response
            return Ok(new PagedResponse<IEnumerable<PostResponseDto>>
                (postsDto, page, pageSize.Value, total: totalPosts));
        }

        // Get api/<controller>/1
        /// <summary>
        /// get a post with id
        /// </summary>
        /// <param name="id">post id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        //[ResponseCache(CacheProfileName = "30SecCache")]
        public async Task<ActionResult<Response<PostResponseDto>>> GetById(int id)
        {
            Post post = await _blogDbContext.Posts.FindAsync(id);
            if (post is null)
                return NotFound(new Response<PostResponseDto>($"Couldn't find any post with ID {id}."));

            var postDto = new PostResponseDto
            {
                ID = post.ID,
                Author = post.Author,
                Title = post.Title,
                Body = post.Body,
                Description = post.Description,
                Thumbnail = post.Thumbnail,
                Created = post.Created,
                Modified = post.Modified,
                Tags = post.Tags
            };
            return Ok(new Response<PostResponseDto>(postDto));
        }

        // Post api/<controller>/
        /// <summary>
        /// add new post
        /// </summary>
        /// <param name="postDto">post data</param>
        /// <returns>created post id</returns>
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

        // Put api/<controller>/1
        /// <summary>
        /// update a post
        /// </summary>
        /// <param name="id">post id</param>
        /// <param name="postDto">new post data</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<Response<int>>> Put(int id, PostDto postDto)
        {
            var post = await _blogDbContext.Posts.FindAsync(id).ConfigureAwait(false);
            if (post is null)
                return NotFound(new Response<int>($"Couldn't find any post with ID {id}."));

            post.Author = postDto.Author;
            post.Title = postDto.Title;
            post.Body = postDto.Body;
            post.Description = postDto.Description;
            post.Tags = postDto.Tags;
            post.Modified = DateTime.Now;
            post.Thumbnail = postDto.Thumbnail;

            _blogDbContext.Posts.Update(post);
            await _blogDbContext.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new Response<int>(post.ID));
        }

        // Delete api/<controller>/1
        /// <summary>
        /// delete a post
        /// </summary>
        /// <param name="id">post id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<int>>> DeleteAsync(int id)
        {
            // first time, do find query And Track the Record for future request's, so ef give it from the cache in next find's.
            var post = await _blogDbContext.Posts
                .FindAsync(id);
            if (post is null)
                return NotFound(new Response<int>($"Couln't find post with ID {id}."));
            // do delete and save
            _blogDbContext.Posts.Remove(post);
            await _blogDbContext.SaveChangesAsync().ConfigureAwait(false);
            return Ok(new Response<int>(id));
        }

        // GET api/<controller>/search/text
        /// <summary>
        /// search in blog posts
        /// </summary>
        /// <param name="term">term/text to search in post's title</param>
        /// <returns></returns>
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
                Modified = p.Modified,
                Tags = p.Tags,
                Thumbnail = p.Thumbnail
            });
            return Ok(new Response<IEnumerable<PostResponseDto>>(postsDto));
        }
    }
}
