using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Server.DTOs.Blog
{
    /// <summary>
    /// The Blog post input model, used for add/edit posts data
    /// </summary>
    public class PostDto
    {
        /// <summary>
        /// post short/header text
        /// </summary>
        [Required(ErrorMessage = "Post Title is required")]
        [MaxLength(50, ErrorMessage = "No more than 50 character allowed!")]
        public string Title { get; set; }

        /// <summary>
        /// Text to display in the url/address bar
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// post content data
        /// </summary>
        [Required(ErrorMessage = "Post Body is required")]
        [MaxLength(5000, ErrorMessage = "No more than 5000 character allowed!")]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }

        /// <summary>
        /// post tags, each tag seperated by semicolon (;)
        /// </summary>
        [MaxLength(254, ErrorMessage = "No more than 254 character allowed!")]
        public string Tags { get; set; }

        /// <summary>
        /// post explanation or footer
        /// </summary>
        [MaxLength(2500, ErrorMessage = "No more than 2500 character allowed!")]
        public string Description { get; set; }

        /// <summary>
        /// post thumbnail image preview (small picture)
        /// </summary>
        [MaxLength(512, ErrorMessage = "No more than 512 character allowed!")]
        [DataType(DataType.ImageUrl)]
        public string Thumbnail { get; set; }
    }
}
