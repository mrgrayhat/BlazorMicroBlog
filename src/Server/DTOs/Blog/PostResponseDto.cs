using System;

namespace MicroBlog.Server.DTOs.Blog
{
    /// <summary>
    /// the post response dto, used for send result to the client
    /// </summary>
    public class PostResponseDto
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
        public string Author { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }

    }
}
