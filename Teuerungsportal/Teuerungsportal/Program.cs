using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Teuerungsportal;
using Teuerungsportal.Resources;
using Teuerungsportal.Services;
using Teuerungsportal.Services.Interfaces;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<StoreService, ApiStoreService>();
builder.Services.AddScoped<CategoryService, ApiCategoryService>();

var host = builder.Build();
await host.SetDefaultCulture();
await host.RunAsync();