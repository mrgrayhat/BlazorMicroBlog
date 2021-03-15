using System;
using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.Auth;
using MicroBlog.Blazor.Client.Services.Http;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Pages.account
{
    public partial class Login
    {
        private UserLoginDto _userForAuthentication = new UserLoginDto();
        [Inject]
        public ToastService _toastService { get; set; }
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
                _toastService.ShowToast("Something is wrong! Please double check again.", ToastLevel.ERROR);
                StateHasChanged();
            }
            else
            {
                _toastService.ShowToast("You have logged in successfully", ToastLevel.SUCCESS);
                NavigationManager.NavigateTo("/", true);
            }
        }
    }
}
