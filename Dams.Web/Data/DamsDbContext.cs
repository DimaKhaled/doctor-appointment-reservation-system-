using Dams.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dams.Web.Data;

public class DamsDbContext(DbContextOptions<DamsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureSpecializations(modelBuilder);
        ConfigureClinics(modelBuilder);
        ConfigurePatients(modelBuilder);
        ConfigureDoctors(modelBuilder);
        ConfigureAdmins(modelBuilder);
        ConfigureSchedules(modelBuilder);
        ConfigureAppointmentSlots(modelBuilder);
        ConfigureAppointments(modelBuilder);
        ConfigureReviews(modelBuilder);
        SeedPhaseZeroData(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_Users_Email");
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Gender).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Users_Gender", "[Gender] IN ('Male', 'Female')");
                t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Patient', 'Doctor', 'Admin')");
            });
        });
    }

    private static void ConfigureSpecializations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(e => e.SpecializationId);
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_Specializations_Name");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });
    }

    private static void ConfigureClinics(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.ClinicId);
            entity.HasIndex(e => e.ClinicName).IsUnique().HasDatabaseName("UQ_Clinics_ClinicName");
            entity.Property(e => e.ClinicName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(250).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
        });
    }

    private static void ConfigurePatients(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId);
            entity.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_Patients_UserId");
            entity.Property(e => e.DateOfBirth).HasColumnType("date");
            entity.Property(e => e.BloodType).HasMaxLength(5);
            entity.Property(e => e.Allergies).HasMaxLength(500);
            entity.Property(e => e.ChronicDiseases).HasMaxLength(500);
            entity.Property(e => e.ProfilePicturePath).HasMaxLength(300);
            entity.HasOne(e => e.User)
                .WithOne(e => e.Patient)
                .HasForeignKey<Patient>(e => e.UserId)
                .HasConstraintName("FK_Patients_Users")
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(t => t.HasCheckConstraint("CK_Patients_DOB", "[DateOfBirth] <= CAST(GETDATE() AS DATE)"));
        });
    }

    private static void ConfigureDoctors(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId);
            entity.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_Doctors_UserId");
            entity.HasIndex(e => e.SpecializationId).HasDatabaseName("IX_Doctors_SpecializationId");
            entity.HasIndex(e => e.ClinicId).HasDatabaseName("IX_Doctors_ClinicId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Doctors_Status");
            entity.Property(e => e.Qualifications).HasMaxLength(1000);
            entity.Property(e => e.ExperienceYears).HasDefaultValue(0);
            entity.Property(e => e.Biography).HasMaxLength(1000);
            entity.Property(e => e.ProfilePicturePath).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue(AppStatuses.Active).IsRequired();
            entity.HasOne(e => e.User)
                .WithOne(e => e.Doctor)
                .HasForeignKey<Doctor>(e => e.UserId)
                .HasConstraintName("FK_Doctors_Users")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Specialization)
                .WithMany(e => e.Doctors)
                .HasForeignKey(e => e.SpecializationId)
                .HasConstraintName("FK_Doctors_Specializations")
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Clinic)
                .WithMany(e => e.Doctors)
                .HasForeignKey(e => e.ClinicId)
                .HasConstraintName("FK_Doctors_Clinics")
                .OnDelete(DeleteBehavior.NoAction);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Doctors_Experience", "[ExperienceYears] >= 0");
                t.HasCheckConstraint("CK_Doctors_Status", "[Status] IN ('Active', 'Inactive')");
            });
        });
    }

    private static void ConfigureAdmins(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId);
            entity.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_Admins_UserId");
            entity.HasOne(e => e.User)
                .WithOne(e => e.Admin)
                .HasForeignKey<Admin>(e => e.UserId)
                .HasConstraintName("FK_Admins_Users")
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSchedules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId);
            entity.HasIndex(e => e.DoctorId).HasDatabaseName("IX_Schedules_DoctorId");
            entity.Property(e => e.DayOfWeek).HasMaxLength(10).IsRequired();
            entity.Property(e => e.StartTime).HasColumnType("time(0)");
            entity.Property(e => e.EndTime).HasColumnType("time(0)");
            entity.HasOne(e => e.Doctor)
                .WithMany(e => e.Schedules)
                .HasForeignKey(e => e.DoctorId)
                .HasConstraintName("FK_Schedules_Doctors")
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Schedules_TimeRange", "[StartTime] < [EndTime]");
                t.HasCheckConstraint("CK_Schedules_DayOfWeek", "[DayOfWeek] IN ('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')");
                t.HasCheckConstraint("CK_Schedules_SlotDuration", "[SlotDurationMinutes] IN (15, 30, 45, 60)");
            });
        });
    }

    private static void ConfigureAppointmentSlots(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId);
            entity.HasIndex(e => new { e.DoctorId, e.SlotDate }).HasDatabaseName("IX_Slots_DoctorId_Date");
            entity.HasIndex(e => e.IsBooked).HasDatabaseName("IX_Slots_IsBooked");
            entity.HasIndex(e => new { e.DoctorId, e.SlotDate, e.StartTime }).IsUnique().HasDatabaseName("UQ_Slots_Doctor_DateTime");
            entity.Property(e => e.SlotDate).HasColumnType("date");
            entity.Property(e => e.StartTime).HasColumnType("time(0)");
            entity.Property(e => e.EndTime).HasColumnType("time(0)");
            entity.Property(e => e.IsBooked).HasDefaultValue(false);
            entity.HasOne(e => e.Schedule)
                .WithMany(e => e.AppointmentSlots)
                .HasForeignKey(e => e.ScheduleId)
                .HasConstraintName("FK_Slots_Schedules")
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Doctor)
                .WithMany(e => e.AppointmentSlots)
                .HasForeignKey(e => e.DoctorId)
                .HasConstraintName("FK_Slots_Doctors")
                .OnDelete(DeleteBehavior.NoAction);
            entity.ToTable(t => t.HasCheckConstraint("CK_Slots_TimeRange", "[StartTime] < [EndTime]"));
        });
    }

    private static void ConfigureAppointments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId);
            entity.HasIndex(e => e.SlotId).IsUnique().HasDatabaseName("UQ_Appointments_SlotId");
            entity.HasIndex(e => e.PatientId).HasDatabaseName("IX_Appointments_PatientId");
            entity.HasIndex(e => e.DoctorId).HasDatabaseName("IX_Appointments_DoctorId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Appointments_Status");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue(AppStatuses.Pending).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Patient)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.PatientId)
                .HasConstraintName("FK_Appointments_Patients")
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Doctor)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.DoctorId)
                .HasConstraintName("FK_Appointments_Doctors")
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Slot)
                .WithOne(e => e.Appointment)
                .HasForeignKey<Appointment>(e => e.SlotId)
                .HasConstraintName("FK_Appointments_Slots")
                .OnDelete(DeleteBehavior.NoAction);
            entity.ToTable(t => t.HasCheckConstraint("CK_Appointments_Status", "[Status] IN ('Pending', 'Confirmed', 'Rejected', 'Cancelled', 'Completed')"));
        });
    }

    private static void ConfigureReviews(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.HasIndex(e => new { e.PatientId, e.DoctorId }).IsUnique().HasDatabaseName("UQ_Reviews_Patient_Doctor");
            entity.HasIndex(e => e.DoctorId).HasDatabaseName("IX_Reviews_DoctorId");
            entity.Property(e => e.ReviewText).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.Patient)
                .WithMany(e => e.Reviews)
                .HasForeignKey(e => e.PatientId)
                .HasConstraintName("FK_Reviews_Patients")
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Doctor)
                .WithMany(e => e.Reviews)
                .HasForeignKey(e => e.DoctorId)
                .HasConstraintName("FK_Reviews_Doctors")
                .OnDelete(DeleteBehavior.NoAction);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5");
                t.HasCheckConstraint("CK_Reviews_TextNotEmpty", "LEN(LTRIM(RTRIM([ReviewText]))) > 0");
            });
        });
    }

    private static void SeedPhaseZeroData(ModelBuilder modelBuilder)
    {
        var seedCreatedAt = new DateTime(2026, 6, 24, 0, 0, 0, DateTimeKind.Utc);
        const string seededPasswordHash = "AQAAAAIAAYagAAAAEE6ZSRSJhu5czumHnpiAh8kPdAXAX+E0Z3A+C+ErGmKZyrQecasvTvBNbWG32GUyzA==";

        modelBuilder.Entity<Specialization>().HasData(
            new Specialization { SpecializationId = 1, Name = "Cardiology" },
            new Specialization { SpecializationId = 2, Name = "Neurology" },
            new Specialization { SpecializationId = 3, Name = "Pediatrics" },
            new Specialization { SpecializationId = 4, Name = "Dermatology" },
            new Specialization { SpecializationId = 5, Name = "Orthopedics" }
        );

        modelBuilder.Entity<Clinic>().HasData(
            new Clinic
            {
                ClinicId = 1,
                ClinicName = "DAMS Main Clinic",
                Address = "10 Health Street",
                City = "Cairo",
                PhoneNumber = "01000000000"
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                FullName = "Seed Patient",
                Email = "patient@dams.local",
                PasswordHash = seededPasswordHash,
                PhoneNumber = "01000000001",
                Gender = "Female",
                Role = AppRoles.Patient,
                IsActive = true,
                CreatedAt = seedCreatedAt
            },
            new User
            {
                UserId = 2,
                FullName = "Seed Doctor",
                Email = "doctor@dams.local",
                PasswordHash = seededPasswordHash,
                PhoneNumber = "01000000002",
                Gender = "Male",
                Role = AppRoles.Doctor,
                IsActive = true,
                CreatedAt = seedCreatedAt
            },
            new User
            {
                UserId = 3,
                FullName = "Seed Admin",
                Email = "admin@dams.local",
                PasswordHash = seededPasswordHash,
                PhoneNumber = "01000000003",
                Gender = "Male",
                Role = AppRoles.Admin,
                IsActive = true,
                CreatedAt = seedCreatedAt
            }
        );

        modelBuilder.Entity<Patient>().HasData(new Patient
        {
            PatientId = 1,
            UserId = 1,
            DateOfBirth = new DateTime(1995, 1, 15),
            BloodType = "O+",
            Allergies = "None",
            ChronicDiseases = "None"
        });

        modelBuilder.Entity<Doctor>().HasData(new Doctor
        {
            DoctorId = 1,
            UserId = 2,
            SpecializationId = 1,
            ClinicId = 1,
            Qualifications = "MBBCh, Cardiology specialist",
            ExperienceYears = 8,
            Biography = "Seed doctor account for phase 0 testing.",
            Status = AppStatuses.Active
        });

        modelBuilder.Entity<Admin>().HasData(new Admin
        {
            AdminId = 1,
            UserId = 3
        });
    }
}
