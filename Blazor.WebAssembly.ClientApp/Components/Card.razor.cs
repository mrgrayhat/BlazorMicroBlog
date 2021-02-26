using Application.Shared.DTO.Blog;
using Microsoft.AspNetCore.Components;

namespace Blazor.WebAssembly.ClientApp.Components
{
    public partial class Card
    {
        [Parameter]
        public PostResponseDto Post { get; set; }

        /// <summary>
        /// truncate/trim Text for specific lenght and add ellipse
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

        public static string Truncate(string value, int maxChars = 100)
        {
            return value.Length > maxChars ? value.Substring(0, maxChars) + "..." : value;
        }
    }
}
