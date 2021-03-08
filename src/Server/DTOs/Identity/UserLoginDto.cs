using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Server.DTOs.Identity
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username/Email is required.")]
        public string UsernameOrEmail { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}
