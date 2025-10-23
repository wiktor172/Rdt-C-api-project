using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.FluentValidation;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WorkExperienceOct2024.Client;
using WorkExperienceOct2024.Client.Core.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// these root components boot the Blazor WebAssembly app into index.html
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient that points back to the ASP.NET Core host (the same origin)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// typed HttpClient for anything else we want to call via DI later
builder.Services.AddHttpClient("HostApi", c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// wire up the two small services that wrap the server endpoints
builder.Services.AddScoped<MarketService>();
builder.Services.AddScoped<StocksService>();

// optional UI library setup (blazorise) for styling and validation goodies
builder.Services.AddBlazorise(options =>
        {
            options.Immediate = true;
            options.ProductToken = "9AC7-58E6-6467-4B22-BF9B-F6CC-F57E";
        })
        .AddBootstrap5Providers()
        .AddFontAwesomeIcons()
        .AddBlazoriseFluentValidation();

await builder.Build().RunAsync();
