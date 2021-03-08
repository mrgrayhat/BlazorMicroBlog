using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.Auth;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Pages.account
{
    public partial class Logout
    {
        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await AuthenticationService.Logout();
            NavigationManager.NavigateTo("/");
        }
    }
}
