using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using AutomationOfPurchases.Client.Auth; // ������ �������� ����, �� ���� ��� CustomAuthStateProvider
using System.Net.Http.Json;
using AutomationOfPurchases.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient, ������� ������ ������ API:
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5156") });

// 1) ������ ������ �����������
builder.Services.AddAuthorizationCore();

// 2) �������� CustomAuthStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();
