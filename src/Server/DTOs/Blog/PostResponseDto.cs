using System;
using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Server.DTOs.Blog
{
    /// <summary>
    /// Blog Post Result Model, include post information/data
    /// </summary>
    public class PostResponseDto
    {
        /// <summary>
        /// the post id in db
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// post short/header text
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// post content data
        /// </summary>
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }
        /// <summary>
        /// creation time
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        /// <summary>
        /// latest edit/update
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? Modified { get; set; }
        /// <summary>
        /// post writer
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// post tags, each tag seperated by semicolon (;)
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        /// post explanation or footer
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// post thumbnail image preview (small picture)
        /// </summary>
        public string Thumbnail { get; set; }

    }
}
