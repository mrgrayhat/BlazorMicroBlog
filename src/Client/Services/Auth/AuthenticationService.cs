using System.Threading.Tasks;
using Blazored.LocalStorage;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace MicroBlog.Blazor.Client.Services.Auth
{
    public interface IAuthenticationService
    {
        Task<RegistrationResponseDto> RegisterUser(UserRegistrationDto userForRegistration);
        Task<LoginResponseDto> Login(UserLoginDto userForAuthentication);
        Task<string> RefreshToken();
        Task Logout();
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IAccountsClient _accountsClient;
        //private readonly ITokenClient _tokenClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthenticationService(ILogger<AuthenticationService> logger, IAccountsClient accountsClient, /*ITokenClient tokenClient,*/ AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage)
        {
            _logger = logger;
            _accountsClient = accountsClient;
            //_tokenClient = tokenClient;
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

                //_logger.LogInformation("Auth token: {refreshToken}", authResult.Token);
                //_logger.LogInformation("Auth refresh token: {refreshToken}", authResult.RefreshToken);

                await _localStorage.SetItemAsync("authToken", authResult.Token).ConfigureAwait(false);
                //await _localStorage.SetItemAsync("refreshToken", authResult.RefreshToken);

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
            //await _localStorage.RemoveItemAsync("refreshToken");

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

        public Task<string> RefreshToken()
        {
            //var token = await _localStorage.GetItemAsync<string>("authToken").ConfigureAwait(false);
            //LoginResponseDto refreshResult = new LoginResponseDto();
            //try
            //{
            //    refreshResult = await _tokenClient.RefreshAsync(new RefreshTokenDto
            //    {
            //        Token = token,
            //        RefreshToken = await _localStorage.GetItemAsync<string>("refreshToken")
            //    }).ConfigureAwait(false);

            //    if (!refreshResult.IsAuthSuccessful)
            //        throw new System.Exception(refreshResult.ErrorMessage);
            //    _logger.LogInformation("Refreshed Token: {refreshToken}", refreshResult.Token);

            //    await _localStorage.SetItemAsync("authToken", refreshResult.Token).ConfigureAwait(false);
            //    await _localStorage.SetItemAsync("refreshToken", refreshResult.RefreshToken).ConfigureAwait(false);

            //    return refreshResult.Token;
            //}
            //catch (ApiException<LoginResponseDto> ex)
            //{
            //    _logger.LogInformation("refresh token failed, response: {refreshToken}", ex.Response);
            //    refreshResult = ex.Result;
            //    return refreshResult.Token;
            //}
            throw new System.NotImplementedException();
        }
    }
}
