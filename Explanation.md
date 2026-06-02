# 🏗️ Detaillierte Projekt-Architektur

## 📐 Architektur-Übersicht (Layers)

```
┌──────────────────────────────────────────────────────────────┐
│                       FRONTEND (Blazor)                       │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  Pages (.razor)                                              │
│  ├─ /doctor.razor              (Doctor Dashboard)           │
│  ├─ /patient/dashboard.razor   (Patient Meine Termine)     │
│  ├─ /admin/users/Index.razor   (Admin User Manager)        │
│  └─ /patients.razor            (Patienten-Liste)           │
│                    ↓                                         │
│  Services (HTTP Clients)                                    │
│  ├─ IAppointmentService        (Termine)                   │
│  ├─ IPatientService            (Patienten)                 │
│  ├─ IUserService               (Benutzer)                  │
│  ├─ IConsultationHourService   (Sprechstunden)            │
│  └─ IAuthService               (Authentifizierung)         │
│                    ↓ HTTP REST                              │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                      BACKEND (ASP.NET Core)                   │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  Controllers (API Endpoints)                                │
│  ├─ AppointmentsController     (POST/GET /api/appointments)│
│  ├─ PatientsController         (POST/GET /api/patients)    │
│  ├─ UsersController            (POST/GET /api/users)       │
│  ├─ DoctorsController          (POST/GET /api/doctors)     │
│  └─ AuthController             (GET /api/auth/me)          │
│                    ↓                                         │
│  Services (Business Logic)                                  │
│  ├─ IAppointmentService        (Termine buchen, validieren)│
│  ├─ IPatientService            (Patient CRUD)              │
│  ├─ IUserService               (User CRUD)                 │
│  ├─ IConsultationHourService   (Sprechstunden laden)      │
│  └─ IDoctorService             (Ärzte verwalten)           │
│                    ↓                                         │
│  Repositories (Data Access)                                 │
│  ├─ IAppointmentRepository     (Appointments in DB)        │
│  ├─ IPatientRepository         (Patients in DB)            │
│  ├─ IUserRepository            (Users in DB)               │
│  ├─ IDoctorRepository          (Doctors in DB)             │
│  └─ IConsultationHourRepository (Consultation Hours in DB) │
│                    ↓ Entity Framework Core                  │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                   DATABASE (SQL Server)                       │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  Tables                                                      │
│  ├─ Users                      (Authentifizierung)          │
│  ├─ Patients                   (Patienten)                  │
│  ├─ Doctors                    (Ärzte)                      │
│  ├─ Appointments               (Termine)                    │
│  ├─ ConsultationHours          (Sprechstunden)             │
│  ├─ MedicalSpecialties         (Fachbereiche)              │
│  └─ Logs (Triggers)            (Audit Logging)             │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

## 🔄 Datenfluss: Ein vollständiges Beispiel

### **Szenario: Patient bucht Termin**

```
┌─────────────────────────────────────────────────────────┐
│ FRONTEND: /patient/dashboard.razor                      │
├─────────────────────────────────────────────────────────┤
│ 1. User wählt:                                          │
│    - Sprechstunde: "Kardiologie Vormittag"             │
│    - Datum: 2025-06-05                                 │
│    - Uhrzeit: 09:15                                    │
│                                                         │
│ 2. Click: "✅ Termin buchen"                           │
│    @onclick="BookAppointment"                          │
└─────────────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────────────┐
│ FRONTEND SERVICE: AppointmentService                    │
├─────────────────────────────────────────────────────────┤
│ public async Task<AppointmentDto?> CreateAppointmentAsync(
│     AppointmentCreateDto dto)
│ {
│     var response = await _http.PostAsJsonAsync(
│         "api/appointments",
│         dto);
│     
│     if (response.IsSuccessStatusCode)
│         return await response.Content
│             .ReadFromJsonAsync<AppointmentDto>();
│     
│     return null;
│ }
│                                                         │
│ → JSON POST zu Backend                                 │
└─────────────────────────────────────────────────────────┘
           ↓ HTTP POST /api/appointments
┌─────────────────────────────────────────────────────────┐
│ BACKEND CONTROLLER: AppointmentsController              │
├─────────────────────────────────────────────────────────┤
│ [Authorize(Policy = "DoctorOderStaffOderAdmin")]       │
│ [HttpPost]                                              │
│ public async Task<IActionResult> Create(
│     [FromBody] AppointmentCreateDto dto)
│ {
│     try
│     {
│         var appointment = 
│             await _appointmentService.CreateAsync(dto);
│         
│         return CreatedAtAction(
│             nameof(Get),
│             new { id = appointment.Id },
│             appointment);
│     }
│     catch (InvalidOperationException ex)
│     {
│         return Conflict(
│             new { message = ex.Message });
│     }
│ }
│                                                         │
│ → Validierung: Ist User autorisiert?                   │
│ → Ruft Service auf                                      │
│ → Fehlerbehandlung                                      │
└─────────────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────────────┐
│ BACKEND SERVICE: AppointmentService                     │
├─────────────────────────────────────────────────────────┤
│ public async Task<Appointment> CreateAsync(
│     AppointmentCreateDto dto)
│ {
│     // 1. Prüfe: Zeitslot bereits gebucht?
│     var collision = 
│         await _appointmentRepository.HasConflictAsync(
│             dto.ConsultationHourId,
│             dto.Date,
│             dto.Time);
│     
│     if (collision)
│         throw new InvalidOperationException(
│             "Dieser Zeitslot ist bereits vergeben.");
│     
│     // 2. Prüfe: Doppelbuchung?
│     var doubleBooking = 
│         await _appointmentRepository
│             .HasDoubleBookingAsync(
│                 dto.PatientId,
│                 dto.Date);
│     
│     if (doubleBooking)
│         throw new InvalidOperationException(
│             "Patient hat bereits Termin heute.");
│     
│     // 3. Erstelle Termin
│     var appointment = new Appointment
│     {
│         PatientId = dto.PatientId,
│         ConsultationHourId = dto.ConsultationHourId,
│         Date = dto.Date,
│         Time = dto.Time
│     };
│     
│     return await _appointmentRepository
│         .CreateAsync(appointment);
│ }
│                                                         │
│ → Business Logic: Geschäftsregeln erzwingen           │
│ → Ruft Repository auf                                  │
│ → Exception werfen bei Fehler                          │
└─────────────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────────────┐
│ BACKEND REPOSITORY: AppointmentRepository               │
├─────────────────────────────────────────────────────────┤
│ public async Task<bool> HasConflictAsync(
│     int consultationHourId,
│     DateTime date,
│     TimeSpan time)
│ {
│     return await _db.Appointments.AnyAsync(a =>
│         a.ConsultationHourId == consultationHourId &&
│         a.Date.Date == date.Date &&
│         a.Time == time &&
│         !a.IsCancelled);
│ }
│                                                         │
│ public async Task<Appointment> CreateAsync(
│     Appointment appointment)
│ {
│     _db.Appointments.Add(appointment);
│     await _db.SaveChangesAsync();
│     return appointment;
│ }
│                                                         │
│ → Datenbank-Queries                                    │
│ → Entity Framework Core                                │
│ → Async/Await für Non-Blocking I/O                     │
└─────────────────────────────────────────────────────────┘
           ↓ SQL
┌─────────────────────────────────────────────────────────┐
│ DATABASE: SQL Server                                    │
├─────────────────────────────────────────────────────────┤
│ 1. SELECT ... FROM Appointments WHERE ...              │
│    (Konflikt-Check)                                     │
│                                                         │
│ 2. SELECT ... FROM Appointments WHERE ...              │
│    (Doppelbuchung-Check)                               │
│                                                         │
│ 3. INSERT INTO Appointments (...)                      │
│    VALUES (PatientId=5, ConsultationHourId=2, ...)    │
│                                                         │
│ 4. TRIGGER: trg_Appointments_InsertLog                │
│    → INSERT INTO AppointmentInsertLog (...)            │
│    (Audit Logging automatisch)                         │
│                                                         │
└─────────────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────────────┐
│ RESPONSE: 201 Created                                  │
├─────────────────────────────────────────────────────────┤
│ {                                                       │
│   "id": 42,                                             │
│   "patientId": 5,                                       │
│   "consultationHourId": 2,                             │
│   "date": "2025-06-05T00:00:00",                       │
│   "time": "09:15:00",                                  │
│   "isCancelled": false,                                │
│   "createdAt": "2025-06-02T12:34:56"                  │
│ }                                                       │
└─────────────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────────────┐
│ FRONTEND: Update UI                                    │
├─────────────────────────────────────────────────────────┤
│ if (result != null)
│ {
│     _bookingSuccess = true;
│     _appointments.Add(result);  // UI aktualisieren
│     StateHasChanged();
│ }
│                                                         │
│ User sieht: "✅ Termin erfolgreich gebucht!"         │
└─────────────────────────────────────────────────────────┘
```

---

## 📁 Frontend-Struktur: Page → Service

### **Beispiel 1: Patient Dashboard**

#### **File: `/patient/dashboard.razor`**

```csharp
@page "/patient/dashboard"
@layout ProtectedLayout
@using Frontend.Models
@using Frontend.Services
@inject IAuthService Auth
@inject IAppointmentService AppointmentService
@inject IPatientService PatientService
@inject IConsultationHourService ConsultationHourService
@inject NavigationManager Nav


    📅 Meine Termine
    Willkommen, @Auth.Username


@if (!Auth.IsStaff)
{
    Kein Zugriff.
}
else if (!_initialized)
{
    Lade...
}
else
{
    
    Meine gebuchten Termine
    @foreach (var a in _appointments.OrderBy(x => x.Date))
    {
        
            @(a.Date.Day.ToString("D2"))
            
                @a.ConsultationHourName
                
                    @a.Date.ToString("dd.MM.yyyy") · @a.Time.ToString(@"hh\:mm")
                
            
            
                ❌ Absagen
            
        
    }
}

@code {
    int _patientId = 0;
    bool _initialized = false;
    List _appointments = new();
    List _consultationHours = new();

    protected override async Task OnInitializedAsync()
    {
        // 1. Laden: Sprechstunden
        _consultationHours = await ConsultationHourService.GetAllAsync();

        // 2. Laden: Eigener Patient-Record
        if (!string.IsNullOrEmpty(Auth.Username))
        {
            var patient = await PatientService.GetMyPatientAsync(Auth.Username);
            if (patient != null)
            {
                _patientId = patient.Id;
                await LoadMyAppointments();
            }
        }

        _initialized = true;
    }

    async Task LoadMyAppointments()
    {
        if (_patientId == 0) return;
        _appointments = await AppointmentService
            .GetPatientAppointmentsAsync(_patientId);
    }

    async Task CancelAppointment(int id)
    {
        var success = await AppointmentService.CancelAppointmentAsync(id);
        if (success)
        {
            await LoadMyAppointments();
        }
    }
}
```

**Erklärung:**
- **@inject**: Dependency Injection der Services
- **OnInitializedAsync()**: Wird einmal beim Laden aufgerufen
- **Service-Calls**: Alle asynchron (await)
- **StateHasChanged()**: Wird implizit aufgerufen nach async operations
- **@foreach**: Rendert die Liste

---

### **Beispiel 2: Admin User Manager**

#### **File: `/admin/users/Index.razor`**

```csharp
@page "/admin/users"
@layout ProtectedLayout
@using Frontend.Models
@using Frontend.Services
@inject IAuthService Auth
@inject IUserService UserService
@inject NavigationManager Nav


    👥 User verwalten


@if (!Auth.IsAdmin)
{
    Kein Zugriff.
}
else if (users == null)
{
    Lade...
}
else
{
    
        @users.Count Benutzer
        
            ➕ Benutzer hinzufügen
        
    

    @foreach (var u in users)
    {
        
            
                @u.Username?.Substring(0, 1).ToUpper()
            
            
                @u.Username
                
                    Rolle: @u.Role · 
                    @(u.IsActive ? "Aktiv" : "Inaktiv")
                
            
            
                
                    ✏️ Bearbeiten
                
                
                    🗑 Löschen
                
            
        
    }
}

@code {
    List? users;

    protected override async Task OnInitializedAsync()
    {
        users = await UserService.GetAllUsersAsync();
    }

    void AddUser() => Nav.NavigateTo("/admin/users/add");
    void EditUser(int id) => Nav.NavigateTo($"/admin/users/edit/{id}");

    async Task DeleteUser(int id)
    {
        var success = await UserService.DeleteUserAsync(id);
        if (success)
        {
            users?.RemoveAll(x => x.Id == id);
        }
    }
}
```

**Wichtige Punkte:**
- **Authorization**: `@if (!Auth.IsAdmin)` prüft Rechte
- **CRUD-Operationen**: Edit, Delete über Service
- **Navigation**: `Nav.NavigateTo()` für Seiten-Links

---

## 🔌 Frontend Services (HTTP Clients)

### **Beispiel: AppointmentService**

#### **File: `Frontend/Services/AppointmentService.cs`**

```csharp
using Frontend.Models;
using System.Net.Http.Json;
namespace Frontend.Services;

public class AppointmentService : IAppointmentService
{
    private readonly HttpClient _http;

    // HttpClient wird via DI injiziert
    // (mit UseDefaultCredentials=true für Kerberos)
    public AppointmentService(HttpClient http)
    {
        _http = http;
    }

    // ✅ GET alle Termine
    public async Task<List> GetAllAppointmentsAsync()
    {
        try 
        { 
            return await _http.GetFromJsonAsync<List>(
                "api/appointments") ?? new(); 
        }
        catch 
        { 
            return new(); 
        }
    }

    // ✅ GET Termine für Patient
    public async Task<List> GetPatientAppointmentsAsync(int patientId)
    {
        try 
        { 
            return await _http.GetFromJsonAsync<List>(
                $"api/appointments/patient/{patientId}") ?? new(); 
        }
        catch 
        { 
            return new(); 
        }
    }

    // ✅ GET freie Zeitslots
    public async Task<List> GetFreeSlotsAsync(
        int consultationHourId, DateTime date)
    {
        try
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            return await _http.GetFromJsonAsync<List>(
                $"api/appointments/free-slots/{consultationHourId}/{dateStr}") 
                ?? new();
        }
        catch 
        { 
            return new(); 
        }
    }

    // ✅ POST neuer Termin
    public async Task CreateAppointmentAsync(
        AppointmentCreateDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/appointments", 
                dto);
            
            if (response.IsSuccessStatusCode)
                return await response.Content
                    .ReadFromJsonAsync();
        }
        catch { }
        
        return null;
    }

    // ✅ POST Termin absagen
    public async Task CancelAppointmentAsync(int id)
    {
        try
        {
            var response = await _http.PostAsync(
                $"api/appointments/cancel/{id}", 
                null);
            
            return response.IsSuccessStatusCode;
        }
        catch 
        { 
            return false; 
        }
    }

    // ✅ PUT Termin aktualisieren
    public async Task UpdateAppointmentAsync(
        int id, AppointmentDto appointment)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                $"api/appointments/{id}", 
                appointment);
            
            return response.IsSuccessStatusCode;
        }
        catch 
        { 
            return false; 
        }
    }

    // ✅ DELETE Termin löschen
    public async Task DeleteAppointmentAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync(
                $"api/appointments/{id}");
            
            return response.IsSuccessStatusCode;
        }
        catch 
        { 
            return false; 
        }
    }
}
```

**Was ist hier wichtig?**

1. **Error Handling**: `try-catch` gibt Default-Wert zurück
2. **REST Verben**: GET (Daten), POST (erstellen), PUT (ändern), DELETE
3. **Async**: Alle HTTP-Calls sind non-blocking
4. **JSON Serialization**: `GetFromJsonAsync()` / `PostAsJsonAsync()`

---

## 🎯 Backend-Struktur: Controller → Service → Repository

### **Ebene 1: Controller (API Endpoint)**

#### **File: `Backend/Controllers/AppointmentsController.cs`**

```csharp
using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // ✅ GET /api/appointments
    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet]
    public async Task Get()
    {
        var appointments = await _appointmentService.GetAllAsync();
        return Ok(appointments);
    }

    // ✅ GET /api/appointments/my-appointments
    [Authorize(Policy = "NurStaff")]
    [HttpGet("my-appointments")]
    public async Task GetMyAppointments()
    {
        // Aktuellen User auslesen aus Claims
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        
        if (string.IsNullOrWhiteSpace(username))
            return Unauthorized();

        try
        {
            var appointments = await _appointmentService
                .GetByPatientUsernameAsync(username);
            
            return Ok(appointments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ✅ GET /api/appointments/free-slots/{id}/{date}
    [Authorize(Policy = "DoctorOderAdmin")]
    [HttpGet("free-slots/{consultationHourId}/{date}")]
    public async Task GetFreeSlots(
        int consultationHourId, string date)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(new { message = "Ungültiges Datum." });

        var slots = await _appointmentService
            .GetFreeSlotsAsync(consultationHourId, parsedDate);
        
        return Ok(slots);
    }

    // ✅ POST /api/appointments
    [Authorize(Policy = "DoctorOderStaffOderAdmin")]
    [HttpPost]
    public async Task Create([FromBody] AppointmentCreateDto dto)
    {
        try
        {
            var appointment = await _appointmentService.CreateAsync(dto);
            
            return CreatedAtAction(
                nameof(Get),
                new { id = appointment.Id },
                appointment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ✅ POST /api/appointments/cancel/{id}
    [Authorize(Policy = "DoctorOderStaffOderAdmin")]
    [HttpPost("cancel/{id}")]
    public async Task Cancel(int id)
    {
        try
        {
            await _appointmentService.CancelAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // ✅ DELETE /api/appointments/{id}
    [Authorize(Policy = "NurAdmin")]
    [HttpDelete("{id}")]
    public async Task Delete(int id)
    {
        await _appointmentService.DeleteAsync(id);
        return Ok();
    }
}
```

**Controller Verantwortungen:**
- ✅ **HTTP Handling**: Request/Response
- ✅ **Authorization**: `[Authorize(Policy = "...")]`
- ✅ **Validierung Input**: `DateOnly.TryParse()`
- ✅ **Error Handling**: Try-Catch + HTTP Status Codes
- ✅ **JSON Serialization**: ASP.NET Core macht das automatisch

**Status Codes:**
- `200 OK` - Erfolgreich
- `201 Created` - Ressource erstellt
- `400 BadRequest` - Ungültige Eingabe
- `401 Unauthorized` - Nicht authentifiziert
- `403 Forbidden` - Autorisierung fehlgeschlagen
- `404 NotFound` - Ressource nicht gefunden
- `409 Conflict` - Geschäftsregel verletzt

---

### **Ebene 2: Service (Business Logic)**

#### **File: `Backend/Services/AppointmentService.cs`**

```csharp
using Backend.Model;
using Backend.Repositories;
namespace Backend.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IConsultationHourRepository _consultationHourRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IConsultationHourRepository consultationHourRepository,
        IPatientRepository patientRepository,
        IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _consultationHourRepository = consultationHourRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
    }

    // ✅ GET: Alle Termine
    public async Task<List> GetAllAsync()
        => await _appointmentRepository.GetAllAsync();

    // ✅ GET: Termine eines Patienten
    public async Task<List> GetByPatientIdAsync(int patientId)
        => await _appointmentRepository.GetByPatientIdAsync(patientId);

    // ✅ GET: Termine über Username
    public async Task<List> GetByPatientUsernameAsync(string username)
    {
        // 1. User via Username suchen
        var user = await _userRepository.GetByUsernameAsync(username)
            ?? throw new KeyNotFoundException(
                $"User {username} nicht gefunden.");

        // 2. Patient via UserId suchen
        var patient = await _patientRepository.GetByUserIdAsync(user.Id)
            ?? throw new KeyNotFoundException(
                $"Patient für User {username} nicht gefunden.");

        // 3. Termine des Patienten laden
        return await _appointmentRepository.GetByPatientIdAsync(patient.Id);
    }

    // ✅ POST: Neuer Termin (mit Validierung!)
    public async Task CreateAsync(AppointmentCreateDto dto)
    {
        // 1. VALIDATION: Zeitslot-Konflikt?
        var collision = await _appointmentRepository.HasConflictAsync(
            dto.ConsultationHourId,
            dto.Date,
            dto.Time);
        
        if (collision)
            throw new InvalidOperationException(
                "Dieser Zeitslot ist bereits vergeben.");

        // 2. VALIDATION: Doppelbuchung (Patient 2x am gleichen Tag)?
        var doubleBooking = await _appointmentRepository.HasDoubleBookingAsync(
            dto.PatientId,
            dto.Date);
        
        if (doubleBooking)
            throw new InvalidOperationException(
                "Patient hat bereits einen Termin an diesem Tag.");

        // 3. CREATE: Termin erstellen
        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            ConsultationHourId = dto.ConsultationHourId,
            Date = dto.Date,
            Time = dto.Time
        };

        return await _appointmentRepository.CreateAsync(appointment);
    }

    // ✅ POST: Termin absagen
    public async Task CancelAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException(
                $"Appointment mit ID {id} nicht gefunden.");

        appointment.IsCancelled = true;
        await _appointmentRepository.UpdateAsync(appointment);
    }

    // ✅ GET: Freie Zeitslots
    public async Task<List> GetFreeSlotsAsync(
        int consultationHourId, DateOnly date)
    {
        // 1. Sprechstunde laden
        var ch = await _consultationHourRepository.GetByIdAsync(consultationHourId);
        
        if (ch == null || !ch.IsActive) 
            return new();

        // 2. Gebuchte Slots laden
        var booked = await _appointmentRepository
            .GetBookedSlotsAsync(consultationHourId, date);

        // 3. Alle 15-Minuten-Slots durchgehen
        var result = new List();
        var current = ch.StartTime;

        while (current < ch.EndTime)
        {
            // Wenn nicht gebucht, zu freien Slots hinzufügen
            if (!booked.Contains(current))
                result.Add(current.ToString(@"hh\:mm"));

            // Nächster 15-Minuten-Slot
            current = current.Add(TimeSpan.FromMinutes(15));
        }

        return result;
    }
}
```

**Service Verantwortungen:**
- ✅ **Business Logic**: Geschäftsregeln erzwingen
- ✅ **Validierung**: Konflikte prüfen
- ✅ **Koordination**: Mehrere Repositories nutzen
- ✅ **Exception Handling**: Aussagekräftige Fehler werfen
- ✅ **Transaktionen**: Konsistenz sichern (via SaveChangesAsync)

**Wichtiges Pattern:**
```csharp
var user = await _userRepository.GetByUsernameAsync(username)
    ?? throw new KeyNotFoundException(...);
```
**`??`** = Null-Coalescing Operator
- Wenn `null`: Exception werfen
- Wenn nicht `null`: Variable zuweisen

---

### **Ebene 3: Repository (Data Access)**

#### **File: `Backend/Repositories/AppointmentRepository.cs`**

```csharp
using Backend.Model;
using Microsoft.EntityFrameworkCore;
namespace Backend.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly DoctorDbContext _db;

    public AppointmentRepository(DoctorDbContext db)
    {
        _db = db;
    }

    // ✅ SELECT: Termin via ID
    public async Task GetByIdAsync(int id)
        => await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

    // ✅ SELECT: Alle Termine
    public async Task<List> GetAllAsync()
        => await _db.Appointments.ToListAsync();

    // ✅ SELECT: Termine eines Patienten
    public async Task<List> GetByPatientIdAsync(int patientId)
        => await _db.Appointments
            .Where(a => a.PatientId == patientId && !a.IsCancelled)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync();

    // ✅ SELECT: Gebuchte Zeitslots an einem Tag
    public async Task<List> GetBookedSlotsAsync(
        int consultationHourId, DateOnly date)
    {
        return await _db.Appointments
            .Where(a => a.ConsultationHourId == consultationHourId
                     && DateOnly.FromDateTime(a.Date) == date
                     && !a.IsCancelled)
            .Select(a => a.Time)
            .ToListAsync();
    }

    // ✅ CHECK: Zeitkonflikt?
    public async Task HasConflictAsync(
        int consultationHourId, DateTime date, TimeSpan time)
    {
        return await _db.Appointments.AnyAsync(a =>
            a.ConsultationHourId == consultationHourId &&
            a.Date.Date == date.Date &&
            a.Time == time &&
            !a.IsCancelled);
    }

    // ✅ CHECK: Doppelbuchung?
    public async Task HasDoubleBookingAsync(
        int patientId, DateTime date)
    {
        return await _db.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.Date.Date == date.Date &&
            !a.IsCancelled);
    }

    // ✅ INSERT: Neuen Termin erstellen
    public async Task CreateAsync(Appointment appointment)
    {
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }

    // ✅ UPDATE: Termin ändern
    public async Task UpdateAsync(Appointment appointment)
    {
        _db.Appointments.Update(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }

    // ✅ DELETE: Termin löschen
    public async Task DeleteAsync(int id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
        }
    }
}
```

**Repository Verantwortungen:**
- ✅ **CRUD**: Create, Read, Update, Delete
- ✅ **LINQ Queries**: DbSet-Abfragen
- ✅ **SaveChangesAsync**: DB Änderungen persistieren
- ✅ **Keine Business Logic**: Nur DB Access
- ✅ **Type Safety**: Kompilzeit-Prüfung via LINQ

**LINQ Pattern Beispiel:**

```csharp
// ❌ FALSCH: SQL Injection Risk
var appointments = _db.Appointments
    .FromSqlInterpolated($"SELECT * FROM Appointments WHERE PatientId = {patientId}");

// ✅ RICHTIG: Safe LINQ
var appointments = await _db.Appointments
    .Where(a => a.PatientId == patientId)
    .ToListAsync();
```

---

## 🔗 Dependency Injection Setup

#### **File: `Backend/Program.cs`**

```csharp
using Backend.Auth;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ───────────────────────────────────────────────────────
// 1️⃣ DATABASE
// ───────────────────────────────────────────────────────
builder.Services.AddDbContext(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ───────────────────────────────────────────────────────
// 2️⃣ AUTHENTICATION (Kerberos)
// ───────────────────────────────────────────────────────
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddScoped();

// ───────────────────────────────────────────────────────
// 3️⃣ REPOSITORIES
// ───────────────────────────────────────────────────────
builder.Services.AddScoped();
builder.Services.AddScoped();
builder.Services.AddScoped();
builder.Services.AddScoped();
builder.Services.AddScoped();

// ───────────────────────────────────────────────────────
// 4️⃣ SERVICES
// ───────────────────────────────────────────────────────
builder.Services.AddScoped();
builder.Services.AddScoped();
builder.Services.AddScoped();
builder.Services.AddScoped();
builder.Services.AddScoped();

// ───────────────────────────────────────────────────────
// 5️⃣ AUTHORIZATION POLICIES
// ───────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NurAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("NurDoctor", p => p.RequireRole("Doctor"));
    options.AddPolicy("NurStaff", p => p.RequireRole("Staff"));
    options.AddPolicy("DoctorOderStaff", p => p.RequireRole("Doctor", "Staff"));
    options.AddPolicy("DoctorOderAdmin", p => p.RequireRole("Doctor", "Admin"));
    options.AddPolicy("DoctorOderStaffOderAdmin", 
        p => p.RequireRole("Doctor", "Staff", "Admin"));
    
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ───────────────────────────────────────────────────────
// 6️⃣ API & CORS
// ───────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5001", "http://192.168.68.50:5001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Was macht Dependency Injection hier?**

```csharp
// Beim Request kommt ein Request an:
// POST /api/appointments

// ASP.NET Core macht:
// 1. Schaut: AppointmentsController benötigt IAppointmentService
// 2. Schaut: IAppointmentService ist registriert als AppointmentService
// 3. Schaut: AppointmentService benötigt mehrere Repositories
// 4. Erstellt: Alle Dependencies automatisch
// 5. Injiziert: In den Controller Constructor

var controller = new AppointmentsController(
    new AppointmentService(
        new AppointmentRepository(dbContext),
        new ConsultationHourRepository(dbContext),
        new PatientRepository(dbContext),
        new UserRepository(dbContext)
    )
);
```

**Lifetime Scopes:**
- `AddScoped` = Neue Instanz pro HTTP Request
- `AddSingleton` = Eine Instanz für alle (Stateless OK)
- `AddTransient` = Neue Instanz jedes mal (selten)

---

## 📋 Zusammenfassung: Layer Verantwortungen

```
┌─────────────────────────────────────────────────────────┐
│ PAGE (.razor)                                           │
│ Verantwortung: UI Rendering, Event Handling            │
│ - Nimmt Benutzer Input                                 │
│ - Ruft Service Methoden auf                            │
│ - Updated UI basierend auf Daten                       │
└─────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│ SERVICE (Frontend)                                      │
│ Verantwortung: HTTP Communication                      │
│ - Ruft Backend API auf via HttpClient                  │
│ - Serialisiert/Deserialisiert JSON                     │
│ - Error Handling (try-catch)                           │
│ - Gibt DTOs an Page zurück                             │
└─────────────────────────────────────────────────────────┘
         ↓ HTTP REST
┌─────────────────────────────────────────────────────────┐
│ CONTROLLER                                              │
│ Verantwortung: HTTP Request/Response Handling          │
│ - Empfängt HTTP Request                                │
│ - Prüft Authorization                                  │
│ - Validiert Input                                      │
│ - Ruft Service auf                                     │
│ - Sendet HTTP Response                                 │
└─────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│ SERVICE (Backend)                                       │
│ Verantwortung: Business Logic                          │
│ - Geschäftsregeln durchsetzen                          │
│ - Validierungen                                        │
│ - Mehrere Repositories koordinieren                    │
│ - Exceptions werfen bei Fehler                         │
└─────────────────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────────┐
│ REPOSITORY                                              │
│ Verantwortung: Data Access (CRUD)                      │
│ - Direkt auf Database zugreifen via EF Core           │
│ - LINQ Queries schreiben                               │
│ - Keine Business Logic!                                │
│ - SaveChangesAsync aufrufen                            │
└─────────────────────────────────────────────────────────┘
         ↓ SQL
┌─────────────────────────────────────────────────────────┐
│ DATABASE                                                │
│ Verantwortung: Persistenz                              │
│ - Speichert Daten                                      │
│ - Enforces Constraints                                 │
│ - Triggers für Audit Logging                           │
└─────────────────────────────────────────────────────────┘
```

---

# 🎯 Herausforderungen und wichtige Erkenntnisse

Während der Entwicklung des Arztverwaltungssystems mussten verschiedene technische Probleme gelöst werden. Die folgenden Punkte waren für das Projekt besonders wichtig.

---

## Rollen- und Rechteverwaltung

Ein zentraler Bestandteil der Anwendung ist die Verwaltung unterschiedlicher Benutzerrollen.

Folgende Rollen wurden umgesetzt:

* Administrator
* Arzt
* Mitarbeiter

Jede Rolle besitzt unterschiedliche Berechtigungen.

### Beispiel

```csharp
[Authorize(Policy = "NurAdmin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    await _userService.DeleteAsync(id);
    return Ok();
}
```

Nur Administratoren dürfen Benutzer löschen.

---

## Vermeidung von Doppelbuchungen

Bei der Terminverwaltung darf ein Zeitslot nicht mehrfach vergeben werden.

Vor jeder Terminbuchung prüft das System:

* Ist der Zeitslot bereits belegt?
* Hat der Patient bereits einen Termin am selben Tag?

### Beispiel

```csharp
if (collision)
{
    throw new InvalidOperationException(
        "Dieser Zeitslot ist bereits vergeben.");
}
```

Dadurch bleiben die Terminpläne konsistent.

---

## Kerberos-Authentifizierung

Die Benutzer werden über ihre Windows-Anmeldung authentifiziert.

Dadurch ist keine zusätzliche Anmeldung innerhalb der Anwendung notwendig.

### Beispiel

```csharp
builder.Services
    .AddAuthentication(
        NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
```

---

## Trennung von Frontend und Backend

Die Anwendung wurde in mehrere Schichten aufgeteilt.

```text
Blazor Frontend
       │
       ▼
ASP.NET Core API
       │
       ▼
Service Layer
       │
       ▼
Repository Layer
       │
       ▼
SQL Server
```

Diese Struktur verbessert Wartbarkeit und Erweiterbarkeit.

---

## Datenbankintegrität

Die Datenbank stellt sicher, dass keine ungültigen oder doppelten Datensätze entstehen.

### Beispiel

```csharp
modelBuilder.Entity<User>()
    .HasIndex(x => x.Username)
    .IsUnique();
```

Dadurch kann jeder Benutzername nur einmal vergeben werden.

---

## Persönliche Erkenntnisse

Während der Entwicklung habe ich gelernt:

* Aufbau einer mehrschichtigen Softwarearchitektur
* Entwicklung von REST-APIs mit ASP.NET Core
* Arbeiten mit Entity Framework Core
* Umsetzung von Rollen- und Rechtekonzepten
* Verwendung von Dependency Injection
* Kommunikation zwischen Frontend und Backend
* Fehleranalyse und Debugging komplexerer Anwendungen

Besonders interessant war die Umsetzung der Terminverwaltung, da dabei Frontend, Backend und Datenbank gemeinsam arbeiten müssen.

---

# Fazit

Das Projekt umfasst die Entwicklung einer vollständigen Arztverwaltungsanwendung mit:

* Benutzerverwaltung
* Patientenverwaltung
* Terminverwaltung
* Rollen- und Rechteverwaltung
* Kerberos-Authentifizierung
* SQL-Server-Datenbank

Durch die klare Trennung von Frontend, Backend und Datenbank entstand eine wartbare und erweiterbare Anwendung, die typische Anforderungen moderner Verwaltungssoftware erfüllt.

Das Projekt hat mir einen praxisnahen Einblick in die professionelle Entwicklung von C#-Anwendungen mit ASP.NET Core, Blazor und SQL Server gegeben.
