using Backend.Auth;
using Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Datenbank ────────────────────────────────────────────────
builder.Services.AddDbContext<DoctorDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Kerberos / Windows Auth ──────────────────────────────────
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// Transformer: liest Rolle aus DB nach Kerberos-Login
builder.Services.AddScoped<IClaimsTransformation, KerberosRollenTransformer>();
builder.Services.AddScoped<UserRepository>();

// ── Autorisierung mit Rollen ─────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NurAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("NurDoctor", p => p.RequireRole("Doctor"));
    options.AddPolicy("NurStaff", p => p.RequireRole("Staff"));
    options.AddPolicy("DoctorOderStaff", p => p.RequireRole("Doctor", "Staff"));
    options.AddPolicy("DoctorOderAdmin", p => p.RequireRole("Doctor", "Admin"));

    // Jeder muss eingeloggt + in DB sein
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ── Services ─────────────────────────────────────────────────
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://192.168.68.202:5038",  // Frontend auf Raspi
                "http://192.168.68.102:5040",  // Backend selbst
                "http://localhost:5000",        // lokale Entwicklung
                "https://localhost:7000"        // lokale Entwicklung HTTPS
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // wichtig für Kerberos!
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthentication(); // muss vor UseAuthorization stehen!
app.UseAuthorization();
app.MapControllers();

app.Run();