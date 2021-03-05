using System;

namespace Application.Server.API.Models.Blog
{
    public class Post
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Modified { get; set; }
        public string Author { get; set; }
        public string Tags { get; set; }
        public string Thumbnail { get; set; }
        public string Description { get; set; }

    }
}
