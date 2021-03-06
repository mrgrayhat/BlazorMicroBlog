using System.Threading.Tasks;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components;
using MicroBlog.Blazor.Client.Services;

namespace MicroBlog.Blazor.Client.Components
{
    public partial class Card
    {
        [Parameter]
        public PostResponseDto Post { get; set; }
        [Inject]
        private NavigationManager Navigation { get; set; }
        [Inject]
        private IBlogClient _blogClient { get; set; }
        [Inject]
        private ToastService _toastService { get; set; }

        private void EditPost(int id)
        {
            Navigation.NavigateTo($"/edit/{id}");
        }
        private async Task DeletePost(int id)
        {
            await _blogClient.DeleteAsync(id).ConfigureAwait(false);
            _toastService.ShowToast($"Post with id {id} Deleted Successfully", ToastLevel.SUCCESS);
            Navigation.NavigateTo("/", true);
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
