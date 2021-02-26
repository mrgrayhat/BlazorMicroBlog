using System.ComponentModel.DataAnnotations;

namespace Application.Shared.DTO.Blog
{
    public class PostDto
    {
        [Required(ErrorMessage = "Post Title is required")]
        [MaxLength(150)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Post Body is required")]
        [MaxLength(5000)]
        public string Body { get; set; }
        [Required(ErrorMessage = "Post Author is required")]
        public string Author { get; set; }
        public string[] Tags { get; set; }
        public string Description { get; set; }
        [MaxLength(500)]
        public string Thumbnail { get; set; }
    }
}
