﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.Auth;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Pages.account
{
    public partial class Registration
    {
        private UserRegistrationDto UserRegistrationDto = new UserRegistrationDto();
        [Inject]
        private IAuthenticationService AuthenticationService { get; set; }
        [Inject]
        public ToastService _toastService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public bool ShowRegistrationErrors { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public async Task Register()
        {
            ShowRegistrationErrors = false;

            var result = await AuthenticationService.RegisterUser(UserRegistrationDto);
            if (!result.IsSuccessfulRegistration)
            {
                Errors = result.Errors;
                ShowRegistrationErrors = true;
                _toastService.ShowToast("Something is wrong! Please double check again.", ToastLevel.ERROR);
                StateHasChanged();
            }
            else
            {
                NavigationManager.NavigateTo("/account/login");
            }
        }
    }
}
