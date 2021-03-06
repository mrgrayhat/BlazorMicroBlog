using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Components
{
    public partial class Pagination
    {
        [Parameter]
        public Paging Paging { get; set; }
        [Parameter]
        public int Spread { get; set; }
        [Parameter]
        public EventCallback<int> SelectedPage { get; set; }

        private List<PagingLink> _links;

        protected override void OnParametersSet()
        {
            CreatePaginationLinks();
        }
        private void CreatePaginationLinks()
        {
            _links = new List<PagingLink>
            {
                new PagingLink(Paging.CurrentPage - 1, Paging.HasPrevious, "Previous")
            };
            for (int i = 1; i <= Paging.TotalPages; i++)
            {
                if (i >= Paging.CurrentPage - Spread && i <= Paging.CurrentPage + Spread)
                {
                    _links.Add(new PagingLink(i, true, i.ToString()) { Active = Paging.CurrentPage == i });
                }
            }
            _links.Add(new PagingLink(Paging.CurrentPage + 1, Paging.HasNext, "Next"));
        }
        private async Task OnSelectedPage(PagingLink link)
        {
            if (link.Page == Paging.CurrentPage || !link.Enabled)
                return;
            Paging.CurrentPage = link.Page;
            await SelectedPage.InvokeAsync(link.Page);
        }
    }

    public class PagingLink
    {
        public string Text { get; set; }
        public int Page { get; set; }
        public bool Enabled { get; set; }
        public bool Active { get; set; }
        public PagingLink(int page, bool enabled, string text)
        {
            Page = page;
            Enabled = enabled;
            Text = text;
        }
    }
    public class Paging
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
