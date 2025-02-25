using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace AutomationOfPurchases.Client.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Читаємо токен із localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            // Якщо токена немає – створюємо анонімного користувача
            if (string.IsNullOrWhiteSpace(token))
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymous);
            }

            // Якщо токен є, можна розпарсити його та створити ClaimsIdentity
            // (Нижче – спрощено. У реальному випадку розбирайте JWT, перевіряйте термін дії і т.д.)
            var claims = new List<Claim> {
                new Claim("token", token), // Формальний claim, або ж Name, Role
                // Наприклад, new Claim(ClaimTypes.Name, "SomeUser");
            };

            var identity = new ClaimsIdentity(claims, "jwtAuth");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        // Викликаємо цей метод після логіну/логауту, щоб повідомити Blazor, що авторизація змінилась
        public void NotifyUserAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
