using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Pos.Client;
using Pos.Client.Servicos;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("A configuração 'ApiBaseUrl' não foi encontrada em wwwroot/appsettings.json.");

builder.Services.AddTransient<ManipuladorAutenticacao>();

builder.Services.AddHttpClient("Pos.Api", cliente => cliente.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<ManipuladorAutenticacao>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Pos.Api"));

builder.Services.AddScoped<ArmazenamentoToken>();
builder.Services.AddScoped<ProviderEstadoAutenticacao>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ProviderEstadoAutenticacao>());
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<ServicoAutenticacao>();
builder.Services.AddScoped<ServicoProdutos>();
builder.Services.AddScoped<ServicoAtalhos>();
builder.Services.AddScoped<ServicoUsuario>();
builder.Services.AddScoped<ServicoVendas>();
builder.Services.AddScoped<ArmazenamentoCarrinho>();

await builder.Build().RunAsync();
