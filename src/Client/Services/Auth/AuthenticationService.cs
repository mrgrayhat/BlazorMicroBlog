using System.Net.Http;
using System.Net.Http.Headers;
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
        //private readonly HttpClient _client;
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
            var authResult = await _accountsClient.LoginAsync(userLoginDto);
            if (!authResult.IsAuthSuccessful)
                return authResult;
            await _localStorage.SetItemAsync("authToken", authResult.Token);
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(userLoginDto.UsernameOrEmail);
            //_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authResult.Token);
            return new LoginResponseDto { IsAuthSuccessful = true };
        }
        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            //_client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<RegistrationResponseDto> RegisterUser(UserRegistrationDto userForRegistration)
        {
            var registerResult = await _accountsClient.RegisterUserAsync(userForRegistration);
            if (!registerResult.IsSuccessfulRegistration)
            {
                return registerResult;
            }
            return new RegistrationResponseDto { IsSuccessfulRegistration = true };
        }
    }
}
