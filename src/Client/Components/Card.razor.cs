using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Components
{
    public partial class Card
    {
        [Parameter]
        public PostResponseDto Post { get; set; }
        [Inject]
        private NavigationManager Navigation { get; set; }
        [Inject]
        private IBlogClient BlogClient { get; set; }
        [Inject]
        private ToastService ToastService { get; set; }
        public bool isLoading { get; set; } = false;

        private void EditPost(int id)
        {
            Navigation.NavigateTo($"/blog/edit/{id}");
        }
        private async Task DeletePost(int id)
        {
            isLoading = true;
            try
            {
                var result = await BlogClient.DeleteAsync(id).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    ToastService.ShowToast($"Post with id {id} Deleted Successfully", ToastLevel.SUCCESS);
                    Navigation.NavigateTo("/", true);
                }
            }
            catch (ApiException<ResponseOfInteger> ex)
            {
                ToastService.ShowToast($"Couldn't Delete Post, due to {ex.Result.Message}", ToastLevel.ERROR);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// truncate/trim sentence words for specific lenght + add ellipse
        /// </summary>
        /// <param name="value">string to truncate</param>
        /// <param name="maxChars">max character to process</param>
        /// <returns></returns>
        private static string TruncateForDisplay(string value, int maxChars = 100)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            string returnValue = string.Empty;
            if (value.Length > maxChars)
            {
                string tmp = value.Substring(0, length: maxChars);
                if (tmp.LastIndexOf(' ') > 0)
                    returnValue = tmp.Substring(0, tmp.LastIndexOf(' ')) + " ...";
            }
            return returnValue;
        }

        /// <summary>
        /// character truncate
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxChars"></param>
        /// <returns></returns>
        public static string Truncate(string value, int maxChars = 100)
        {
            return value.Length > maxChars ? value.Substring(0, maxChars) + "..." : value;
        }
    }
}
