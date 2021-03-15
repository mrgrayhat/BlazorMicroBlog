using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MicroBlog.Server.DTOs.Identity
{
    /// <summary>
    /// user register input model
    /// </summary>
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "A unique username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "A unique Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password doesn't match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Display(Name = "I agree to the terms and conditions")]
        [Compare("IsTrue", ErrorMessage = "Please agree to Terms and Conditions")]
        public bool AcceptEULA { get; set; }

        /// <summary>
        /// Just to check if Accept EULA is accepted or not, Not include in data
        /// </summary>
        [JsonIgnore]
        public bool IsTrue => true;
    }
}
