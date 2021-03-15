using System;

namespace MicroBlog.Server.DTOs.User
{
    public class UserResponseDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public string Sex { get; set; }
        public string Country { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public int Followers { get; set; }
        public int Posts { get; set; }
        public string Email { get; set; }

    }
}
