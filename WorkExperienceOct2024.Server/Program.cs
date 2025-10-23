using System.Net.Http.Headers;
using WorkExperienceOct2024.Server.Interfaces;
using WorkExperienceOct2024.Server.Options;
using WorkExperienceOct2024.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("alphavantage", c =>
{
    c.BaseAddress = new Uri("https://www.alphavantage.co/");
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    c.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.Configure<AlphaVantageOptions>(builder.Configuration.GetSection("AlphaVantage"));
builder.Services.AddScoped<IStocksProvider, AlphaVantageService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

