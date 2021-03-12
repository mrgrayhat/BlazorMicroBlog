using System.Collections.Generic;

namespace MicroBlog.Server.DTOs.Identity
{
    public class RegistrationResponseDto
    {
        public RegistrationResponseDto()
        {
            IsSuccessfulRegistration = true;
        }
        public RegistrationResponseDto(IEnumerable<string> errors)
        {
            IsSuccessfulRegistration = false;
            Errors = errors;
        }

        public bool IsSuccessfulRegistration { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
