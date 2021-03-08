using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MicroBlog.Server.DTOs.Identity
{
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "I agree to the terms and conditions")]
        [Compare("IsTrue", ErrorMessage = "Please agree to Terms and Conditions")]
        public bool AcceptEULA { get; set; }
        
        [JsonIgnore]
        public bool IsTrue => true;
    }
}
