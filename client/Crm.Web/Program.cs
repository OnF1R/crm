using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Crm.Web.Services;
using Crm.Web.Auth;
using Crm.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBase"] ?? "";
if (!string.IsNullOrEmpty(apiBase))
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });
else
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
