using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using AutomationOfPurchases.Client.Auth; // ��� ������ ����
// ���� using ...

namespace AutomationOfPurchases.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // 1) �������� ��������� DelegatingHandler
            builder.Services.AddScoped<CustomAuthHeaderHandlerWith401>();

            // 2) ����� ������ ��������� sp.GetRequiredService<CustomAuthHeaderHandlerWith401>()
            builder.Services.AddScoped(sp =>
            {
                var customHandler = sp.GetRequiredService<CustomAuthHeaderHandlerWith401>();

                // �������! ������, �� "�������" ���������� ���� HttpClientHandler
                customHandler.InnerHandler = new HttpClientHandler();

                // ����� ��������� HttpClient ������ ������ ���������� handler
                return new HttpClient(customHandler)
                {
                    BaseAddress = new Uri("http://localhost:5156")
                };
            });

            // 3) �����������
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}
