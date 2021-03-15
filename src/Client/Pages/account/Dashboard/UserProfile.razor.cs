using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace MicroBlog.Blazor.Client.Pages.account.Dashboard
{
    public partial class UserProfile
    {
        [Inject]
        private ILogger<UserProfile> _logger { get; set; }
        [Inject]
        private ToastService ToastService { get; set; }
        [Inject]
        private IAccountsClient _accountClient { get; set; }
        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }

        [Parameter]
        public string Username { get; set; }
        private ResponseOfUserResponseDto UserDto { get; set; }
        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrWhiteSpace(Username))
                await GetUserInfo(Username);
            else
            {
                var authState = await AuthState;
                if (authState != null && authState.User.Identity.IsAuthenticated)
                {
                    await GetUserInfo(authState.User.Identity.Name);
                }
            }
            if (UserDto is null)
            {
                UserDto = new ResponseOfUserResponseDto();
            }
        }

        private async Task GetUserInfo(string username)
        {
            UserDto = new ResponseOfUserResponseDto();
            try
            {
                UserDto = await _accountClient
                                .GetAsync(username).ConfigureAwait(false);
                _logger.LogInformation("User Response Info: ", UserDto);


                StateHasChanged();
            }
            catch (ApiException<ResponseOfUserResponseDto> ex)
            {
                ToastService.ShowToast(ex.Result.Message ?? ex.Result.Errors.FirstOrDefault(), ToastLevel.ERROR);
            }

        }
    }
}
