using Backend.Model;
using Microsoft.EntityFrameworkCore;

public class DoctorDbContext : DbContext
{
    public DoctorDbContext(DbContextOptions<DoctorDbContext> options)
        : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MedicalSpecialty> MedicalSpecialties => Set<MedicalSpecialty>();
    public DbSet<ConsultationHour> ConsultationHours => Set<ConsultationHour>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Username)
            .IsUnique();

        modelBuilder.Entity<Patient>()
            .HasIndex(x => x.SocialSecurityNumber)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(x => new { x.ConsultationHourId, x.Date, x.Time })
            .IsUnique();

        // FIX FÜR ENUM ALS STRING
        modelBuilder.Entity<User>()
            .Property(x => x.Role)
            .HasConversion<string>();

        // ✅ TRIGGER-FIX: EF Core informieren dass Trigger existieren
        modelBuilder.Entity<Appointment>()
            .ToTable(tb =>
            {
                tb.HasTrigger("trg_Appointments_InsertLog");
                tb.HasTrigger("trg_Appointments_StatusLog");
            });

        modelBuilder.Entity<Patient>()
            .ToTable(tb =>
            {
                tb.HasTrigger("trg_Patients_AccountLog");
            });
    }
}