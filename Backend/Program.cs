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
                "http://192.168.68.202:5038",
                "http://192.168.68.201:5040",
                "http://localhost:5000",
                "https://localhost:7000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();