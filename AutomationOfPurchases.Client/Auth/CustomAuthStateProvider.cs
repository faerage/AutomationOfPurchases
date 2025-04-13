using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
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
            // 1) Читаємо токен із LocalStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                // Якщо токена немає — повертаємо анонімного користувача
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymous);
            }

            try
            {
                // 2) Розбираємо (декодуємо) JWT без перевірки підпису
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                // 3) Створюємо ClaimsIdentity, вказуючи:
                //    - nameType, щоб ClaimTypes.Name брався з, наприклад, `sub` чи ін.
                //    - roleType, щоб ролі бралися з нашого claim типу (наприклад, 
                //      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").
                //    Залежно від того, що у вас у токені, можна підставити інші константи.
                var identity = new ClaimsIdentity(
                    claims: jwt.Claims,
                    authenticationType: "jwtAuth",
                    nameType: JwtRegisteredClaimNames.Sub, // або "unique_name" чи інший
                    roleType: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                );

                // 4) Створюємо ClaimsPrincipal і повертаємо
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                // Якщо токен "битий" — повертаємо анонімного
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymous);
            }
        }

        /// <summary>
        /// Викликати цей метод після логіну/логауту, 
        /// щоб повідомити Blazor, що авторизація змінилася.
        /// </summary>
        public void NotifyUserAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
