using System.Collections.Generic;
using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Pages.blog
{
    [Authorize(Roles = "Admin,Writer")]
    public partial class EditPost
    {
        [Inject]
        private IBlogClient BlogClient { get; set; }
        [Inject]
        private NavigationManager navigator { get; set; }
        [Inject]
        private ToastService toastService { get; set; }

        /// <summary>
        /// post id
        /// </summary>
        [Parameter]
        public int Id { get; set; }
        public bool ShowErrors { get; set; } = false;
        public IEnumerable<string> Errors { get; set; }

        private PostDto PostDto { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var old = await BlogClient.GetByIdAsync(Id).ConfigureAwait(false);
            PostDto = new PostDto
            {
                Title = old.Data.Title,
                Body = old.Data.Body,
                Thumbnail = old.Data.Thumbnail,
                Tags = old.Data.Tags,
                Description = old.Data.Description
            };
        }

        private void AssignImageUrl(string imgUrl) => PostDto.Thumbnail = imgUrl;

        protected async void HanleValidSubmit()
        {
            ResponseOfInteger response; // update post response

            try
            {
                response = await BlogClient.PutAsync(Id, PostDto).ConfigureAwait(false);
                if (response.Succeeded)
                {
                    toastService.ShowToast("Post Updated Successfully!", ToastLevel.SUCCESS);
                    navigator.NavigateTo($"/blog/post/{response.Data}");
                }
            }
            catch (ApiException<ResponseOfInteger> ex)
            {
                ShowErrors = true;
                Errors = ex.Result.Errors;
                toastService.ShowToast($"Couldn't Update Post. {ex.Result.Message}", ToastLevel.ERROR);
                StateHasChanged();
            }
        }
    }
}
