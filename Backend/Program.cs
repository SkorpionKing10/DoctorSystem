using Backend.Auth;
using Backend.Repositories;
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

builder.Services.AddScoped<IClaimsTransformation, KerberosRollenTransformer>();

// ── Repositories ─────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IConsultationHourRepository, ConsultationHourRepository>();

// ── Services ─────────────────────────────────────────────────
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IConsultationHourService, ConsultationHourService>();

// ── Autorisierung ────────────────────────────────────────────
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

// ── API ──────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS ─────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://192.168.68.50:5001",
                "http://192.168.68.50:5000",
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