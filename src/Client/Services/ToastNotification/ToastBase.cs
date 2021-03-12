using System;
using Microsoft.AspNetCore.Components;

namespace MicroBlog.Blazor.Client.Services.ToastNotification
{
    public class ToastBase : ComponentBase, IDisposable
    {
        private const string SUCCESS = "Success";
        private const string WARNING = "Warning";
        private const string ERROR = "Error";
        private const string INFO = "Info";

        [Inject] ToastService ToastService { get; set; }

        protected string Heading { get; set; }
        protected string Message { get; set; }
        protected bool IsVisible { get; set; }
        protected string BackgroundCssClass { get; set; }
        protected string IconCssClass { get; set; }

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
            ToastService.OnHide += HideToast;
        }

        private void ShowToast(string message, ToastLevel level)
        {
            BuildToastSettings(level, message);
            IsVisible = true;
            StateHasChanged();
        }

        private void HideToast()
        {
            IsVisible = false;
            StateHasChanged();
        }

        private void BuildToastSettings(ToastLevel level, string message)
        {
            switch (level)
            {
                case ToastLevel.SUCCESS:
                    BackgroundCssClass = "bg-success";
                    IconCssClass = "thumb-up";
                    Heading = SUCCESS;
                    break;
                case ToastLevel.WARNING:
                    BackgroundCssClass = "bg-warning";
                    IconCssClass = "warning";
                    Heading = WARNING;
                    break;
                case ToastLevel.ERROR:
                    BackgroundCssClass = "bg-danger";
                    IconCssClass = "bug";
                    Heading = ERROR;
                    break;
                case ToastLevel.INFO:
                    BackgroundCssClass = "bg-info";
                    IconCssClass = "lightbulb";
                    Heading = INFO;
                    break;
            }

            Message = message;
        }

        public void Dispose()
        {
            ToastService.OnShow -= ShowToast;
        }
    }
}
