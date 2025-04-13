using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace AutomationOfPurchases.Client.Auth
{
    /// <summary>
    /// Кастомний DelegatingHandler, що в кожен HTTP-запит додає заголовок Authorization із токеном.
    /// </summary>
    public class CustomAuthHeaderHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public CustomAuthHeaderHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1) Спробуємо дістати токен із localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            // 2) Якщо токен не порожній – додамо заголовок Authorization
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 3) Викликаємо "наступний" обробник у ланцюжку або безпосередньо відправляємо запит
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
