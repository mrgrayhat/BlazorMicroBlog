using System;
using System.Collections.Generic;
using MicroBlog.Server.API.Models.Blog;
using Microsoft.AspNetCore.Identity;

namespace MicroBlog.Server.Models.Identity
{
    public class UserInfo : IdentityUser
    {
        public string Avatar { get; set; } = @"\site\avatars\no-avatar.png";
        public string Bio { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public string Sex { get; set; } = "Unknown";
        public byte Age { get; set; }
        public string Country { get; set; }
        public string LocaleCulture { get; set; } = "en-US";
        public DateTime? LastActivityDate { get; set; }
        public int Followers { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
