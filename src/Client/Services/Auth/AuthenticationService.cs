using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components.Authorization;

namespace MicroBlog.Blazor.Client.Services.Auth
{
    public interface IAuthenticationService
    {
        Task<RegistrationResponseDto> RegisterUser(UserRegistrationDto userForRegistration);
        Task<LoginResponseDto> Login(UserLoginDto userForAuthentication);
        Task Logout();
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountsClient _accountsClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        public AuthenticationService(IAccountsClient accountsClient, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage)
        {
            _accountsClient = accountsClient;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<LoginResponseDto> Login(UserLoginDto userLoginDto)
        {
            var authResult = new LoginResponseDto();
            try
            {
                authResult = await _accountsClient.LoginAsync(userLoginDto);
                if (!authResult.IsAuthSuccessful)
                    return authResult;
                await _localStorage.SetItemAsync("authToken", authResult.Token);
                ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(authResult.Token);
                return new LoginResponseDto { IsAuthSuccessful = true };
            }
            catch
            {
                return authResult;
            }

        }
        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<RegistrationResponseDto> RegisterUser(UserRegistrationDto userForRegistration)
        {
            var registerResult = await _accountsClient.RegisterUserAsync(userForRegistration).ConfigureAwait(false);
            if (!registerResult.IsSuccessfulRegistration)
            {
                return registerResult;
            }
            return new RegistrationResponseDto { IsSuccessfulRegistration = true };
        }
    }
}
