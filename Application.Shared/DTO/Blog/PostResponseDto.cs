using System;

namespace Application.Shared.DTO.Blog
{
    public class PostResponseDto
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public string Author { get; set; }
        public string[] Tags { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }

    }
}
