using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AutomationOfPurchases.Client.Auth
{
    /// <summary>
    /// DelegatingHandler, який:
    /// 1) Додає Bearer-токен із localStorage,
    /// 2) Якщо отримує 401 Unauthorized – знищує токен і перенаправляє на /login.
    /// </summary>
    public class CustomAuthHeaderHandlerWith401 : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManager;

        public CustomAuthHeaderHandlerWith401(IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // 1) Дістаємо токен із localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 2) Робимо запит до сервера
            var response = await base.SendAsync(request, cancellationToken);

            // 3) Якщо отримали 401 - прибираємо токен і переходимо на логін
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Видаляємо токен
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");

                // Перенаправляємо на /login
                // forceLoad = true зробить повне перезавантаження сторінки
                _navigationManager.NavigateTo("/login", forceLoad: true);
            }

            return response;
        }
    }
}
