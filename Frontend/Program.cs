using Frontend.Components;
using Frontend.Services;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

// ── AuthService mit Kerberos ────────────────────────
builder.Services.AddScoped<IAuthService>(sp =>
{
    var handler = new HttpClientHandler
    {
        UseDefaultCredentials = true
    };
    var http = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.68.50:5000/")
    };
    return new AuthService(http);
});

// ── Services mit Kerberos ───────────────────────────
builder.Services.AddScoped<IAppointmentService>(sp =>
{
    var handler = new HttpClientHandler { UseDefaultCredentials = true };
    var http = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.68.50:5000/")
    };
    return new AppointmentService(http);
});

builder.Services.AddScoped<IPatientService>(sp =>
{
    var handler = new HttpClientHandler { UseDefaultCredentials = true };
    var http = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.68.50:5000/")
    };
    return new PatientService(http);
});

builder.Services.AddScoped<IUserService>(sp =>
{
    var handler = new HttpClientHandler { UseDefaultCredentials = true };
    var http = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.68.50:5000/")
    };
    return new UserService(http);
});

builder.Services.AddScoped<IConsultationHourService>(sp =>
{
    var handler = new HttpClientHandler { UseDefaultCredentials = true };
    var http = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.68.50:5000/")
    };
    return new ConsultationHourService(http);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();