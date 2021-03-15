using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Pages.blog
{
    public partial class AddPost
    {
        private PostDto PostDto { get; set; } = new PostDto();
        [Inject]
        public IBlogClient BlogClient { get; set; }
        [Inject]
        public ToastService _toastService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        public bool ShowErrors { get; set; }
        public IEnumerable<string> Errors { get; set; }

        protected async Task HanleValidSubmit()
        {
            ResponseOfInteger response;
            ShowErrors = false;
            try
            {
                response = await BlogClient.PostAsync(PostDto).ConfigureAwait(false);
                if (response.Succeeded)
                {
                    _toastService.ShowToast("Post Published Successfully!", ToastLevel.SUCCESS);
                    NavigationManager.NavigateTo($"/blog/post/{(int)response.Data}");
                }
            }
            catch (ApiException<ResponseOfInteger> ex)
            {
                ShowErrors = true;
                Errors = ex.Result.Errors;
                _toastService.ShowToast("Couldn't Publish Post", ToastLevel.ERROR);
                StateHasChanged();
            }
        }

        private void AssignImageUrl(string imgUrl) => PostDto.Thumbnail = imgUrl;
    }
}
