namespace MicroBlog.Server.DTOs.Identity
{
    public class LoginResponseDto
    {
        public LoginResponseDto()
        {

        }
        public LoginResponseDto(string error)
        {
            IsAuthSuccessful = false;
            ErrorMessage = error;
        }

        public bool IsAuthSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public string Token { get; set; }
    }
}
