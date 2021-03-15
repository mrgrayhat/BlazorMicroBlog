using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private IEnumerable<string> Errors { get; set; }
        private bool ShowErrors { get; set; } = false;


        protected async Task HandleSelected(InputFileChangeEventArgs e)
        {
            IBrowserFile file = e.File;
            if (file != null && file.Size > 0)
            {
                if (ContentType == UploadContentType.IMAGE || file.ContentType.Contains("image", StringComparison.OrdinalIgnoreCase))
                {
                    var content = new MultipartFormDataContent();
                    ResponseOfString uploadResult; // upload response
                    _uploading = true;
                    try
                    {
                        IBrowserFile resizedImage = await file.RequestImageFileAsync("image/png", 300, 500);
                        using var memoryStream = resizedImage.OpenReadStream(resizedImage.Size);

                        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");

                        content.Add(new StreamContent(memoryStream, Convert.ToInt32(resizedImage.Size)), file.ContentType, file.Name);

                        uploadResult = await UploadClient.PostAsync(new FileParameter(memoryStream, file.Name, file.ContentType)).ConfigureAwait(false);
                        if (uploadResult.Succeeded)
                        {
                            ImgUrl = uploadResult.Data;
                            ToastService.ShowToast("Image uploaded successfully", ToastLevel.SUCCESS);
                        }
                    }
                    catch (ApiException<ResponseOfString> ex)
                    {
                        ShowErrors = true;
                        Errors = ex.Result.Errors;
                        ToastService.ShowToast($"Failed to upload image. {ex.Result.Message ?? ex.Result.Errors.FirstOrDefault()}", ToastLevel.ERROR);
                    }
                    finally
                    {
                        _uploading = false;
                    }
                }

                await OnChange.InvokeAsync(ImgUrl);
            }
            else
            {
                ToastService.ShowToast($"File is empty!", ToastLevel.ERROR);
            }
            StateHasChanged();
        }
    }


    public enum UploadContentType
    {
        IMAGE = 0,
        Media = 1,
        Document = 2
    }
}
