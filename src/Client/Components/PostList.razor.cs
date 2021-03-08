using System;
using System.Threading.Tasks;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Components
{
    public partial class PostList
    {
        [Inject]
        private IBlogClient _blogClient { get; set; }

        private PagedResponseOfIEnumerableOfPostResponseDto PostsList { get; set; }
        public Paging Paging { get; set; } = new Paging();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        protected override async Task OnInitializedAsync()
        {
            await RefreshPostsList();
        }


        private async Task RefreshPostsList()
        {
            PostsList = await _blogClient.IndexAsync(PageSize, Page).ConfigureAwait(false);

            Paging.CurrentPage = PostsList.PageNumber;
            Paging.PageSize = PostsList.PageSize;
            Paging.TotalCount = PostsList.Total;
            Paging.TotalPages = (int)Math.Ceiling(PostsList.Total / (double)PostsList.PageSize);
            StateHasChanged();
        }

        private async void ChangePageSize(ChangeEventArgs e)
        {
            PageSize = int.Parse(e.Value.ToString());
            await RefreshPostsList();
        }

        /// <summary>
        /// using this method we seed some random data to server
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        private async Task FillWithDemoData(int max = 5)
        {
            var rng = new Random();
            for (var i = 0; i < max; i++)
            {
                int rnd = rng.Next();
                await _blogClient.PostAsync(new PostDto
                {
                    Body = $"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. {rnd}",
                    Title = $"post {rnd}",
                    Tags = $"tag{rnd};tag{i + rnd}",
                    Thumbnail = $"https://picsum.photos/200/200/?random={rnd}",
                    Description = "demo data"
                });
            }

            await RefreshPostsList();
        }

        private async Task DeleteAllPosts()
        {
            foreach (var post in PostsList.Data)
            {
                await _blogClient.DeleteAsync(post.Id);
            }

            await RefreshPostsList();
        }

        private async Task SelectedPage(int page)
        {
            Page = page;
            await RefreshPostsList();
        }
    }
}
