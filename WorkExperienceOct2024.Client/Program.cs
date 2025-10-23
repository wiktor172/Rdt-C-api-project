using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.FluentValidation;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WorkExperienceOct2024.Client;
using WorkExperienceOct2024.Client.Core.Services;
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient("HostApi", c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddScoped<MarketService>();

builder.Services.AddBlazorise(options =>
            {
                options.Immediate = true;
                options.ProductToken = "9AC7-58E6-6467-4B22-BF9B-F6CC-F57E";
            })
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons()
            .AddBlazoriseFluentValidation();

// This is what im adding to register the stock service
builder.Services.AddScoped<StocksService>();



await builder.Build().RunAsync();
