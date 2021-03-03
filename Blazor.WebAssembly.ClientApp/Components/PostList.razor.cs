using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Shared.DTO.Blog;
using Application.Shared.Wrappers;
using Microsoft.AspNetCore.Components;

namespace Blazor.WebAssembly.ClientApp.Components
{
    public partial class PostList
    {
        private IEnumerable<PostResponseDto> PostsList { get; set; }
        public Paging Paging { get; set; } = new Paging();

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        protected override async Task OnInitializedAsync()
        {
            await RefreshPostsList();
        }


        private async Task RefreshPostsList()
        {
            var response = await Http.GetFromJsonAsync<PagedResponse<IEnumerable<PostResponseDto>>>($"/api/Blog?page={Page}&pageSize={PageSize}");

            PostsList = response.Data;

            Paging.CurrentPage = response.PageNumber;
            Paging.PageSize = response.PageSize;
            Paging.TotalCount = response.Total;
            Paging.TotalPages = (int)Math.Ceiling(response.Total / (double)response.PageSize);
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
                using var response = await Http.GetAsync($"https://picsum.photos/200/200/?random={rnd}").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                
                await Http.PostAsJsonAsync("/api/Blog", new PostDto
                {
                    Author = $"admin",
                    Body = $@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. {rnd}",
                    Title = $"post {rnd}",
                    Tags = $"tag{rnd};tag{i + rnd}",
                    Thumbnail = response.RequestMessage.RequestUri.ToString()
                });
            }

            await RefreshPostsList();
        }

        private async Task DeletePost(Guid id)
        {
            await Http.DeleteAsync($"/api/Blog/{id}");
            await RefreshPostsList();
        }

        private async Task DeleteAllPosts()
        {
            foreach (var post in PostsList)
            {
                await Http.DeleteAsync($"/api/Blog/{post.ID}");
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
