using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MicroBlog.Server.API.Infrastructure.Contexts;
using MicroBlog.Server.API.Models.Blog;
using MicroBlog.Server.DTOs.Blog;
using MicroBlog.Server.Models.Identity;
using MicroBlog.Server.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace MicroBlog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Writer")]
    public class BlogController : ControllerBase
    {
        private const int DEFAULT_PAGE_SIZE = 10;

        private readonly ILogger<BlogController> _logger;
        private readonly BlogDbContext _blogDbContext;
        private readonly UserManager<UserInfo> _userManager;

        public BlogController(ILogger<BlogController> logger, BlogDbContext blogDbContext, UserManager<UserInfo> userManager)
        {
            _blogDbContext = blogDbContext ?? throw new ArgumentNullException($"{nameof(blogDbContext)} can't be null.");
            _userManager = userManager ?? throw new ArgumentNullException($"{nameof(userManager)} can't be null.");
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
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK, typeof(PagedResponse<IEnumerable<PostResponseDto>>))]
        public async Task<ActionResult<PagedResponse<IEnumerable<PostResponseDto>>>> Index([FromQuery] int? pageSize, int page = 1)
        {
            pageSize = pageSize.GetValueOrDefault(DEFAULT_PAGE_SIZE);
            // get paged result from posts table and sort them by newer posts
            IEnumerable<Post> posts = await _blogDbContext.Posts
                .AsNoTracking() // remove unnecessary tracking cost for larg data
                .Include(a => a.Author)
                .OrderByDescending(x => x.Created)
                .ThenBy(x => x.Modified)
                .Skip((page - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .ToListAsync().ConfigureAwait(false);
            // get total posts number
            int totalBlogPosts = await _blogDbContext.Posts.AsNoTracking()
                .CountAsync().ConfigureAwait(false);
            // map ef query result to a DTO for client
            IEnumerable<PostResponseDto> postsDto = posts.Select(p => new PostResponseDto
            {
                ID = p.ID,
                Author = p.Author.UserName,
                Title = p.Title,
                Body = p.Body,
                Created = p.Created,
                Modified = p.Modified,
                Description = p.Description,
                Tags = p.Tags,
                Thumbnail = p.Thumbnail
            });

            _logger.LogInformation("{count} data sent to client", postsDto.Count());
            // return the paged response
            return Ok(new PagedResponse<IEnumerable<PostResponseDto>>
                (postsDto, page, pageSize.Value, total: totalBlogPosts));
        }

        // Get api/<controller>/1
        /// <summary>
        /// get a post with id
        /// </summary>
        /// <param name="id">post id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Response<PostResponseDto>))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(Response<PostResponseDto>))]
        //[ResponseCache(CacheProfileName = "30SecCache")]
        public async Task<ActionResult<Response<PostResponseDto>>> GetById(int id)
        {
            //Post post = await _blogDbContext.Posts.FindAsync(id);
            Post post = await _blogDbContext.Posts
                .AsNoTracking()
                .Include(a => a.Author)
                .FirstOrDefaultAsync(x => x.ID == id).ConfigureAwait(false);
            if (post is null)
                return NotFound(new Response<PostResponseDto>($"Couldn't find any post with ID {id}."));

            var postDto = new PostResponseDto
            {
                ID = post.ID,
                Author = post.Author.UserName,
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
        [ProducesResponseType(401)]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Response<int>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(Response<int>))]
        public async Task<ActionResult<Response<int>>> Post(PostDto postDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Response<int>
                {
                    Succeeded = false,
                    Message = "Model state is not valid",
                    Errors = ModelState.Values.SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                });
            //if (await _blogDbContext.Posts.FirstOrDefaultAsync(x => x.Title.Equals(postDto.Title)).ConfigureAwait(false) != null)
            //{
            //    ModelState.AddModelError("Title", "A post with this title is exist, specify another unique title.");
            //    return BadRequest(new Response<int>
            //    {
            //        Succeeded = false,
            //        Message = "Post title is not unique.",
            //        Errors = ModelState.Values.SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
            //    });
            //}
            Post mapped = new Post
            {
                Author = await _userManager.FindByNameAsync(User.Identity.Name).ConfigureAwait(false),
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
        /// <param name="id">post id to edit</param>
        /// <param name="postDto">new post data</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(401)]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Response<int>))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(Response<int>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(Response<int>))]

        public async Task<ActionResult<Response<int>>> Put(int id, PostDto postDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Response<int>
                {
                    Succeeded = false,
                    Message = "ModelState is not valid.",
                    Errors = ModelState.Values.SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                });

            //var post = await _blogDbContext.Posts.FindAsync(id).ConfigureAwait(false);
            var post = await _blogDbContext.Posts
                .Include(a => a.Author)
                .FirstOrDefaultAsync(x => x.ID == id).ConfigureAwait(false);
            if (post is null)
                return NotFound(new Response<int>($"Couldn't find any post with ID {id}."));

            post.Author = await _userManager.FindByNameAsync(User.Identity.Name)
                .ConfigureAwait(false);
            post.Title = postDto.Title;
            post.Body = postDto.Body;
            post.Description = postDto.Description;
            post.Tags = postDto.Tags;
            post.Modified = DateTime.Now;
            post.Thumbnail = postDto.Thumbnail;

            _blogDbContext.Posts.Update(post);
            await _blogDbContext.SaveChangesAsync().ConfigureAwait(false);

            //return StatusCode(201, new Response<int>(post.ID));
            return Ok(new Response<int>(post.ID));
        }

        // Delete api/<controller>/1
        /// <summary>
        /// delete a post
        /// </summary>
        /// <param name="id">post id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(401)]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Response<int>))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(Response<int>))]
        public async Task<ActionResult<Response<int>>> DeleteAsync(int id)
        {
            // first time, do find query And Track the Record for future request's, so ef give it from the cache in next find's.
            var post = await _blogDbContext.Posts.FindAsync(id).ConfigureAwait(false);
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
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK, typeof(Response<IEnumerable<PostResponseDto>>))]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(Response<IEnumerable<PostResponseDto>>))]

        public async Task<ActionResult<Response<IEnumerable<PostResponseDto>>>> Search(string term)
        {
            var posts = await _blogDbContext.Posts
                .AsNoTracking()
                .Include(a => a.Author)
                .Where(x => x.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Created)
                .ToListAsync().ConfigureAwait(false);
            if (!posts.Any())
                return NotFound(new Response<PostResponseDto>($"No Match Found with term {term}."));

            var postsDto = posts.Select(p => new PostResponseDto
            {
                ID = p.ID,
                Author = p.Author.UserName,
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
