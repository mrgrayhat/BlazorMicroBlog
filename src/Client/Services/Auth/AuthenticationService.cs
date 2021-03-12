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
            LoginResponseDto authResult = new LoginResponseDto();
            try
            {
                authResult = await _accountsClient.LoginAsync(userLoginDto).ConfigureAwait(false);
                if (!authResult.IsAuthSuccessful)
                    return authResult;

                await _localStorage.SetItemAsync("authToken", authResult.Token).ConfigureAwait(false);
                ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(authResult.Token);

                return authResult;
            }
            catch (ApiException<LoginResponseDto> ex)
            {
                authResult = ex.Result;
                return authResult;
            }

        }
        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken").ConfigureAwait(false);
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<RegistrationResponseDto> RegisterUser(UserRegistrationDto userForRegistration)
        {
            RegistrationResponseDto registerResult = new RegistrationResponseDto();
            try
            {
                registerResult = await _accountsClient.RegisterUserAsync(userForRegistration).ConfigureAwait(false);
                return registerResult;
            }
            catch (ApiException<RegistrationResponseDto> ex)
            {
                registerResult = ex.Result;
                return registerResult;
            }
        }
    }
}
