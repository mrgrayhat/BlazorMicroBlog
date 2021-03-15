using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using MicroBlog.Blazor.Client.Services.ToastNotification;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Logging;
using Toolbelt.Blazor;

namespace MicroBlog.Blazor.Client.Services.Http
{
    /// <summary>
    /// Intercept Http Requests to Add Requirements such as token and refresh token. The SendAsync <see cref="DelegatingHandler"/> will check and execute requests. also handle responses.
    /// 
    /// </summary>
    public class HttpInterceptorService : DelegatingHandler, IDisposable
    {
        private readonly HttpClientInterceptor _interceptor;
        //private readonly NavigationManager _navManager;
        private readonly ILocalStorageService _localStorage;
        private readonly ToastService _toastService;
        private readonly ILogger<HttpInterceptorService> _logger;

        public HttpInterceptorService(ILogger<HttpInterceptorService> logger, ILocalStorageService localStorage, HttpClientInterceptor interceptor/*, NavigationManager navManager*/, ToastService toastService)
        {
            _logger = logger;
            _toastService = toastService;
            _localStorage = localStorage;
            _interceptor = interceptor;
            //_navManager = navManager;
        }

        #region Response handler event register and dispose delegates
        public void RegisterEvent() => _interceptor.AfterSend += InterceptResponse;
        public void DisposeEvent() => _interceptor.AfterSend -= InterceptResponse;
        #endregion

        /// <summary>
        /// execute before any http request (for types/http clients that registered this interceptor)
        /// </summary>
        /// <param name="request">the sending request</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // for browser to send cookies and auth headers
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            // check auth token exist on http request or not.
            if (!request.Headers.Contains("bearer"))
            {
                // set auht tokens for request
                await SetToken(request, cancellationToken);
            }

            // refresh token check
            //var absPath = e.Request.RequestUri.AbsolutePath;
            //if (!absPath.Contains("token") && !absPath.Contains("accounts"))
            //{
            //    var token = await _refreshTokenService.TryRefreshToken();
            //if (!string.IsNullOrEmpty(token))
            //{
            //    e.Request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            //}

            RegisterEvent();
            var response = await base.SendAsync(request, cancellationToken);
            //DisposeEvent();
            return response;
        }

        private void InterceptResponse(object sender, HttpClientInterceptorEventArgs e)
        {
            if (!e.Response.IsSuccessStatusCode)
            {
                string message = string.Empty;
                switch (e.Response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        //_navManager.NavigateTo("/404");
                        message = "The requested resorce was not found.";
                        _toastService.ShowToast(message, ToastLevel.WARNING);
                        break;
                    case HttpStatusCode.BadRequest:
                        message = "Something is wrong! Please double check again.";
                        _toastService.ShowToast(message, ToastLevel.ERROR);
                        break;
                    case HttpStatusCode.Unauthorized:
                        //_navManager.NavigateTo("/account/login");
                        message = "User is not authorized";
                        _toastService.ShowToast(message, ToastLevel.ERROR);
                        break;
                    default:
                        //_navManager.NavigateTo("/500");
                        message = "Something went wrong, please contact Administrator";
                        _toastService.ShowToast(message, ToastLevel.ERROR);
                        break;
                    case HttpStatusCode.NoContent:
                        message = "Request has been successfully processed.";
                        _toastService.ShowToast(message, ToastLevel.INFO);
                        break;
                    case HttpStatusCode.RequestTimeout:
                        message = "Unfortunately, the service is not available at this time.";
                        _toastService.ShowToast(message, ToastLevel.WARNING);
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        message = "Unfortunately, the service is not available at this time.";
                        _toastService.ShowToast(message, ToastLevel.ERROR);
                        break;
                }
                //throw new HttpResponseException(message);
            }
        }

        /// <summary>
        /// get user token from browser local storage, if exist
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetToken()
        {
            return await _localStorage.GetItemAsync<string>("authToken");
        }
        /// <summary>
        /// set auth token to request header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetToken(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            var token = await GetToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            }
        }
    }
}
