using System;
using System.Collections.Generic;
using System.Linq;
using Application.Server.API.Models;
using Application.Shared.DTO.Blog;
using Application.Shared.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Application.Server.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class BlogController : ControllerBase
    {
        private static List<PostResponseDto> BlogPosts { get; set; }
        private const int DEFAULT_PAGE_SIZE = 10;

        private readonly ILogger<BlogController> _logger;

        public BlogController(ILogger<BlogController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// this function just run once.
        /// </summary>
        static BlogController()
        {
            // seed some demo data for blog
            SeedExampleData();
        }

        [HttpPost]
        public ActionResult<Response<Guid>> Post(PostDto postDto)
        {
            var mapped = new PostResponseDto
            {
                Author = postDto.Author,
                Title = postDto.Title,
                Body = postDto.Body,
                Created = DateTime.Now,
                Description = postDto.Description,
                ID = Guid.NewGuid(),
                Tags = postDto.Tags,
                Thumbnail = postDto.Thumbnail
            };
            BlogPosts.Add(mapped);
            return Ok(new Response<Guid>(mapped.ID));
        }

        [HttpGet("{id}")]
        [ResponseCache(CacheProfileName = "60SecCache")]
        public ActionResult<Response<PostResponseDto>> Get(Guid Id)
        {
            if (!Guid.TryParse(Id.ToString(), out _))
                return BadRequest(new Response<PostResponseDto>("invalid post id"));

            var post = BlogPosts.FirstOrDefault(x => x.ID.Equals(Id));
            if (post is null)
                return NotFound(new Response<PostResponseDto>($"Couln't find post with id {Id}."));
            return Ok(new Response<PostResponseDto>(post));
        }

        [HttpDelete("{id}")]
        public ActionResult<Response<Guid>> Delete(Guid Id)
        {
            if (!Guid.TryParse(Id.ToString(), out _))
                return BadRequest(new Response<Guid>("invalid post id"));

            var post = BlogPosts.FirstOrDefault(x => x.ID.Equals(Id));
            if (post is null)
                return NotFound(new Response<Guid>($"Couln't find post with id {Id}."));
            BlogPosts.Remove(post);
            return Ok(new Response<Guid>(Id));
        }

        [HttpGet]
        [ResponseCache(CacheProfileName = "60SecCache")]
        public ActionResult<PagedResponse<IEnumerable<PostResponseDto>>> Get([FromQuery] int? pageSize, int page = 1)
        {
            pageSize = pageSize.GetValueOrDefault(DEFAULT_PAGE_SIZE);
            var data = BlogPosts
                .OrderByDescending(x => x.Created)
                .Skip((page - 1) * pageSize.Value)
                .Take(pageSize.Value);
            _logger.LogInformation("{count} data sent to client", data.Count());

            return Ok(new PagedResponse<IEnumerable<PostResponseDto>>
                (data, page, pageSize.Value, total: BlogPosts.Count));
        }

        private static void SeedExampleData(int count = 20)
        {
            Random rng = new Random();
            BlogPosts = new List<PostResponseDto>();
            for (int i = 0; i < count; i++)
            {
                BlogPosts.Add(new PostResponseDto
                {
                    Created = DateTime.Now,
                    ID = Guid.NewGuid(),
                    Author = ExampleBlogData.PostAuthors[i],
                    Body = ExampleBlogData.PostBodies[i],
                    Title = ExampleBlogData.PostTitles[i],
                    Tags = new string[] { $"tag{rng.Next(1, 100_000)}" },
                    Thumbnail = $"https://picsum.photos/200/200/?random={i}"
                });
            }

            #region random generate
            //IEnumerable<PostResponseDto> data = Enumerable.Range(1, pageSize).Select(index => new PostResponseDto
            //{
            //    Created = DateTime.Now.AddDays(index),
            //    ID = rng.Next(1, 1000),
            //    Author = ExampleBlogData.PostAuthors[rng.Next(ExampleBlogData.PostAuthors.Length)],
            //    Body = ExampleBlogData.PostBodies[rng.Next(ExampleBlogData.PostBodies.Length)],
            //    Title = ExampleBlogData.PostTitles[rng.Next(ExampleBlogData.PostTitles.Length)]
            //});
            #endregion


        }
    }
}
