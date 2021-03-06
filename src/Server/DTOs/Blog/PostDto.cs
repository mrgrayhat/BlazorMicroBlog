using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Server.DTOs.Blog
{
    /// <summary>
    /// The Post Dto, used for add posts or edit them.
    /// </summary>
    public class PostDto
    {
        [Required(ErrorMessage = "Post Title is required")]
        [MaxLength(50, ErrorMessage = "No more than 50 character allowed!")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Post Body is required")]
        [MaxLength(5000, ErrorMessage = "No more than 5000 character allowed!")]
        public string Body { get; set; }
        [Required(ErrorMessage = "Post Author is required")]
        [MaxLength(25, ErrorMessage = "No more than 25 character allowed!")]
        public string Author { get; set; }
        [MaxLength(254, ErrorMessage = "No more than 254 character allowed!")]
        public string Tags { get; set; }
        [MaxLength(2500, ErrorMessage = "No more than 2500 character allowed!")]
        public string Description { get; set; }
        [MaxLength(512, ErrorMessage = "No more than 512 character allowed!")]
        public string Thumbnail { get; set; }
    }
}
