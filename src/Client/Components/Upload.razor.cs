using System;
using System.Net.Http;
using System.Threading.Tasks;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using MicroBlog.BlogClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace MicroBlog.Blazor.Client.Components
{
    [Authorize]
    public partial class Upload
    {
        [Inject]
        private IUploadClient UploadClient { get; set; }
        [Inject]
        private ToastService ToastService { get; set; }

        [Parameter]
        public EventCallback<string> OnChange { get; set; }
        [Parameter]
        public UploadContentType ContentType { get; set; } = UploadContentType.IMAGE;
        [Parameter]
        public string ImgUrl { get; set; }

        private bool _uploading { get; set; } = false;


        protected async Task HandleSelected(InputFileChangeEventArgs e)
        {
            IBrowserFile file = e.File;
            {
                if (file != null && file.Size > 0)
                {
                    if (ContentType == UploadContentType.IMAGE || file.ContentType.Contains("image", StringComparison.OrdinalIgnoreCase))
                    {
                        _uploading = true;
                        IBrowserFile resizedImage = await file.RequestImageFileAsync("image/png", 300, 300);
                        using var memoryStream = resizedImage.OpenReadStream(resizedImage.Size);

                        var content = new MultipartFormDataContent();
                        content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data");
                        content.Add(new StreamContent(memoryStream, Convert.ToInt32(resizedImage.Size)), file.ContentType, file.Name);

                        var result = await UploadClient.PostAsync(new FileParameter(memoryStream, file.Name, file.ContentType));

                        _uploading = false;
                        if (result.Succeeded)
                        {
                            ImgUrl = result.Data;
                            ToastService.ShowToast("Image uploaded successfully", ToastLevel.SUCCESS);
                        }
                        else
                        {
                            _uploading = false;
                            ToastService.ShowToast($"Failed to upload image. {result.Message}", ToastLevel.ERROR);
                        }
                        await OnChange.InvokeAsync(ImgUrl);

                    }
                }
            }
        }

    }

    public enum UploadContentType
    {
        IMAGE = 0,
        Media = 1,
        Document = 2
    }
}
