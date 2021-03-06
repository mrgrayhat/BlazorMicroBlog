namespace MicroBlog.Server.API.Models.Blog
{
    public class File
    {
        public int ID { get; set; }
        public string Creator { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public string Description { get; set; }
    }
}
