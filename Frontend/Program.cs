using Frontend.Components;
using Frontend.Services;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

// HttpClient mit UseDefaultCredentials = Kerberos-Ticket automatisch mitsenden
builder.Services.AddScoped<AuthService>(sp =>
{
    var handler = new HttpClientHandler
    {
        UseDefaultCredentials = true  // ← Kerberos-Ticket wird automatisch mitgeschickt!
    };
    var http = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.68.102:5040/")
    };
    return new AuthService(http);
});

builder.Services.AddScoped<ApiService>(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new ApiService(sp.GetRequiredService<IHttpClientFactory>());
});

// HttpClient für alle anderen Razor-Komponenten (auch mit Kerberos)
builder.Services.AddScoped(sp =>
{
    var handler = new HttpClientHandler
    {
        UseDefaultCredentials = true  // ← Kerberos überall!
    };
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.68.102:5040/")
    };
});

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();