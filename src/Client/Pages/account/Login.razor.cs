using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.Auth;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Pages.account
{
    public partial class Login
    {
        private UserLoginDto _userForAuthentication = new UserLoginDto();

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        public bool ShowAuthError { get; set; }
        public string Error { get; set; }

        public async Task ExecuteLogin()
        {
            ShowAuthError = false;

            LoginResponseDto result = await AuthenticationService.Login(_userForAuthentication);
            if (!result.IsAuthSuccessful)
            {
                Error = result.ErrorMessage;
                ShowAuthError = true;
            }
            else
            {
                NavigationManager.NavigateTo("/");
            }
        }
    }
}
