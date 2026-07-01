using Dams.Web.Models;
using Dams.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Dams.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IApplicationBuilder applicationBuilder)
    {
        using var scope = applicationBuilder.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DamsDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        await context.Database.MigrateAsync();


        if (!await context.Specializations.AnyAsync())
        {
            var specializations = new List<Specialization>
            {
                new() { Name = "Cardiology" },
                new() { Name = "Neurology" },
                new() { Name = "Pediatrics" },
                new() { Name = "Dermatology" },
                new() { Name = "Orthopedics" },
                new() { Name = "Internal Medicine" },
                new() { Name = "Neurosurgery" },
                new() { Name = "Ophthalmology" },
                new() { Name = "Otolaryngology" },
                new() { Name = "Obstetrics & Gynecology (OB-GYN)" }
            };

            await context.Specializations.AddRangeAsync(specializations);
            await context.SaveChangesAsync();
        }


        if (!await context.Clinics.AnyAsync())
        {
            var clinics = new List<Clinic>
            {
                new()
                {
                    ClinicName = "Cairo Medical Center",
                    Address = "15 Abbas El Akkad Street, Nasr City",
                    City = "Cairo",
                    PhoneNumber = "0224011000"
                },

                new()
                {
                    ClinicName = "Alexandria Health Clinic",
                    Address = "45 Fouad Street",
                    City = "Alexandria",
                    PhoneNumber = "034812300"
                },

                new()
                {
                    ClinicName = "Giza Care Hospital",
                    Address = "12 Al Haram Street",
                    City = "Giza",
                    PhoneNumber = "0235874100"
                },

                new()
                {
                    ClinicName = "Mansoura Specialized Clinic",
                    Address = "18 El Gomhoria Street",
                    City = "Mansoura",
                    PhoneNumber = "0502345000"
                },

                new()
                {
                    ClinicName = "Tanta Medical Clinic",
                    Address = "22 Saeed Street",
                    City = "Tanta",
                    PhoneNumber = "0403356100"
                },

                new()
                {
                    ClinicName = "Assiut Family Hospital",
                    Address = "7 El Geish Street",
                    City = "Assiut",
                    PhoneNumber = "0882365000"
                }
            };

            await context.Clinics.AddRangeAsync(clinics);
            await context.SaveChangesAsync();
        }


        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                // Admin
                new()
                {
                    FullName = "System Administrator",
                    Email = "admin@dams.com",
                    PhoneNumber = "01000000001",
                    Gender = "Male",
                    Role = AppRoles.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // Doctors
                new()
                {
                    FullName = "Dr. Ahmed Hassan",
                    Email = "ahmed.hassan@dams.com",
                    PhoneNumber = "01010000001",
                    Gender = "Male",
                    Role = AppRoles.Doctor,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Dr. Sara Mohamed",
                    Email = "sara.mohamed@dams.com",
                    PhoneNumber = "01010000002",
                    Gender = "Female",
                    Role = AppRoles.Doctor,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Dr. Omar Mahmoud",
                    Email = "omar.mahmoud@dams.com",
                    PhoneNumber = "01010000003",
                    Gender = "Male",
                    Role = AppRoles.Doctor,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Dr. Nour El-Din",
                    Email = "nour.eldin@dams.com",
                    PhoneNumber = "01010000004",
                    Gender = "Male",
                    Role = AppRoles.Doctor,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Dr. Mariam Adel",
                    Email = "mariam.adel@dams.com",
                    PhoneNumber = "01010000005",
                    Gender = "Female",
                    Role = AppRoles.Doctor,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Dr. Mostafa Ali",
                    Email = "mostafa.ali@dams.com",
                    PhoneNumber = "01010000006",
                    Gender = "Male",
                    Role = AppRoles.Doctor,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // Patients
                new()
                {
                    FullName = "Mohamed Adel",
                    Email = "mohamed.adel@dams.com",
                    PhoneNumber = "01020000001",
                    Gender = "Male",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Fatma Ahmed",
                    Email = "fatma.ahmed@dams.com",
                    PhoneNumber = "01020000002",
                    Gender = "Female",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Youssef Mahmoud",
                    Email = "youssef.mahmoud@dams.com",
                    PhoneNumber = "01020000003",
                    Gender = "Male",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Mona Samir",
                    Email = "mona.samir@dams.com",
                    PhoneNumber = "01020000004",
                    Gender = "Female",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Omar Tarek",
                    Email = "omar.tarek@dams.com",
                    PhoneNumber = "01020000005",
                    Gender = "Male",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Nourhan Ali",
                    Email = "nourhan.ali@dams.com",
                    PhoneNumber = "01020000006",
                    Gender = "Female",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Khaled Ibrahim",
                    Email = "khaled.ibrahim@dams.com",
                    PhoneNumber = "01020000007",
                    Gender = "Male",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    FullName = "Aya Hassan",
                    Email = "aya.hassan@dams.com",
                    PhoneNumber = "01020000008",
                    Gender = "Female",
                    Role = AppRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Hash the password for every user
            foreach (var user in users)
            {
                user.PasswordHash = passwordService.HashPassword(user, "Dams@123");
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }


        // Seed Admin
        if (!await context.Admins.AnyAsync())
        {
            var adminUser = await context.Users.FirstAsync(u => u.Email == "admin@dams.com");

            await context.Admins.AddAsync(new Admin
            {
                UserId = adminUser.UserId
            });

            await context.SaveChangesAsync();
        }


        // Seed Doctors
        if (!await context.Doctors.AnyAsync())
        {
            await context.Doctors.AddRangeAsync(

                new Doctor
                {
                    UserId = (await context.Users.FirstAsync(u => u.Email == "ahmed.hassan@dams.com")).UserId,
                    SpecializationId = (await context.Specializations.FirstAsync(s => s.Name == "Cardiology")).SpecializationId,
                    ClinicId = (await context.Clinics.FirstAsync(c => c.ClinicName == "Cairo Medical Center")).ClinicId,
                    Qualifications = "MD in Cardiology - Cairo University",
                    ExperienceYears = 15,
                    Biography = "Consultant cardiologist specializing in heart disease, hypertension, and preventive cardiology.",
                    ProfilePicturePath = "/images/doctors/doctor-1.jpg",
                    Status = AppStatuses.Active
                },

                new Doctor
                {
                    UserId = (await context.Users.FirstAsync(u => u.Email == "sara.mohamed@dams.com")).UserId,
                    SpecializationId = (await context.Specializations.FirstAsync(s => s.Name == "Neurology")).SpecializationId,
                    ClinicId = (await context.Clinics.FirstAsync(c => c.ClinicName == "Alexandria Health Clinic")).ClinicId,
                    Qualifications = "MD in Neurology - Alexandria University",
                    ExperienceYears = 12,
                    Biography = "Neurology consultant experienced in stroke management, epilepsy, and neurological disorders.",
                    ProfilePicturePath = "/images/doctors/doctor-2.jpg",
                    Status = AppStatuses.Active
                },

                new Doctor
                {
                    UserId = (await context.Users.FirstAsync(u => u.Email == "omar.mahmoud@dams.com")).UserId,
                    SpecializationId = (await context.Specializations.FirstAsync(s => s.Name == "Orthopedics")).SpecializationId,
                    ClinicId = (await context.Clinics.FirstAsync(c => c.ClinicName == "Giza Care Hospital")).ClinicId,
                    Qualifications = "Orthopedic Surgery Fellowship",
                    ExperienceYears = 11,
                    Biography = "Orthopedic specialist treating fractures, sports injuries, and joint replacement cases.",
                    ProfilePicturePath = "/images/doctors/doctor-3.jpg",
                    Status = AppStatuses.Active
                },

                new Doctor
                {
                    UserId = (await context.Users.FirstAsync(u => u.Email == "nour.eldin@dams.com")).UserId,
                    SpecializationId = (await context.Specializations.FirstAsync(s => s.Name == "Dermatology")).SpecializationId,
                    ClinicId = (await context.Clinics.FirstAsync(c => c.ClinicName == "Mansoura Specialized Clinic")).ClinicId,
                    Qualifications = "Egyptian Board of Dermatology",
                    ExperienceYears = 9,
                    Biography = "Dermatologist specializing in acne treatment, eczema, psoriasis, and cosmetic dermatology.",
                    ProfilePicturePath = "/images/doctors/doctor-4.jpg",
                    Status = AppStatuses.Active
                },

                new Doctor
                {
                    UserId = (await context.Users.FirstAsync(u => u.Email == "mariam.adel@dams.com")).UserId,
                    SpecializationId = (await context.Specializations.FirstAsync(s => s.Name == "Obstetrics & Gynecology (OB-GYN)")).SpecializationId,
                    ClinicId = (await context.Clinics.FirstAsync(c => c.ClinicName == "Tanta Medical Clinic")).ClinicId,
                    Qualifications = "MD in Obstetrics & Gynecology",
                    ExperienceYears = 13,
                    Biography = "OB-GYN consultant providing prenatal care, women's health services, and gynecological surgery.",
                    ProfilePicturePath = "/images/doctors/doctor-5.jpg",
                    Status = AppStatuses.Active
                },

                new Doctor
                {
                    UserId = (await context.Users.FirstAsync(u => u.Email == "mostafa.ali@dams.com")).UserId,
                    SpecializationId = (await context.Specializations.FirstAsync(s => s.Name == "Internal Medicine")).SpecializationId,
                    ClinicId = (await context.Clinics.FirstAsync(c => c.ClinicName == "Assiut Family Hospital")).ClinicId,
                    Qualifications = "MD in Internal Medicine",
                    ExperienceYears = 17,
                    Biography = "Consultant physician managing diabetes, hypertension, thyroid disorders, and chronic diseases.",
                    ProfilePicturePath = "/images/doctors/doctor-6.jpg",
                    Status = AppStatuses.Active
                }

            );

            await context.SaveChangesAsync();
        }


        // Seed Patients
        if (!await context.Patients.AnyAsync())
        {
            var users = await context.Users
                .Where(u => u.Role == AppRoles.Patient)
                .OrderBy(u => u.UserId)
                .ToListAsync();

            var patients = new List<Patient>
            {
                new()
                {
                    UserId = users[0].UserId, // Mohamed Adel
                    DateOfBirth = new DateTime(1998, 5, 14),
                    BloodType = "A+",
                    Allergies = "Penicillin",
                    ChronicDiseases = null,
                    ProfilePicturePath = null
                },

                new()
                {
                    UserId = users[1].UserId, // Fatma Ahmed
                    DateOfBirth = new DateTime(1995, 11, 8),
                    BloodType = "O+",
                    Allergies = "Seafood",
                    ChronicDiseases = "Asthma",
                    ProfilePicturePath = null
                },

                new()
                {
                    UserId = users[2].UserId, // Youssef Mahmoud
                    DateOfBirth = new DateTime(2000, 2, 21),
                    BloodType = "B+",
                    Allergies = null,
                    ChronicDiseases = null,
                    ProfilePicturePath = null
                },

                new()
                {
                    UserId = users[3].UserId, // Mona Samir
                    DateOfBirth = new DateTime(1992, 9, 3),
                    BloodType = "AB+",
                    Allergies = "Dust",
                    ChronicDiseases = "Hypertension",
                    ProfilePicturePath = null
                },

                new()
                {
                    UserId = users[4].UserId, // Omar Tarek
                    DateOfBirth = new DateTime(1988, 12, 17),
                    BloodType = "O-",
                    Allergies = null,
                    ChronicDiseases = "Diabetes",
                    ProfilePicturePath = null
                },

                new()
                {
                    UserId = users[5].UserId, // Nourhan Ali
                    DateOfBirth = new DateTime(1999, 7, 25),
                    BloodType = "A-",
                    Allergies = "Pollen",
                    ChronicDiseases = null,
                    ProfilePicturePath = null
                },

                new()
                {
                    UserId = users[6].UserId, // Khaled Ibrahim
                    DateOfBirth = new DateTime(1985, 4, 11),
                    BloodType = "B-",
                    Allergies = null,
                    ChronicDiseases = "High Cholesterol",
                    ProfilePicturePath = null
                },

                new()
                {
                    UserId = users[7].UserId, // Aya Hassan
                    DateOfBirth = new DateTime(2001, 1, 30),
                    BloodType = "A+",
                    Allergies = "Peanuts",
                    ChronicDiseases = null,
                    ProfilePicturePath = null
                }
            };

            await context.Patients.AddRangeAsync(patients);
            await context.SaveChangesAsync();
        }


        // Seed Schedules
        if (!await context.Schedules.AnyAsync())
        {
            await context.Schedules.AddRangeAsync(

                // Dr. Ahmed Hassan (Cardiology)
                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "ahmed.hassan@dams.com")).DoctorId,
                    DayOfWeek = "Sunday",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(13, 0),
                    SlotDurationMinutes = 30
                },

                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "ahmed.hassan@dams.com")).DoctorId,
                    DayOfWeek = "Tuesday",
                    StartTime = new TimeOnly(15, 0),
                    EndTime = new TimeOnly(19, 0),
                    SlotDurationMinutes = 30
                },

                // Dr. Sara Mohamed (Neurology)
                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "sara.mohamed@dams.com")).DoctorId,
                    DayOfWeek = "Monday",
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(14, 0),
                    SlotDurationMinutes = 30
                },

                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "sara.mohamed@dams.com")).DoctorId,
                    DayOfWeek = "Wednesday",
                    StartTime = new TimeOnly(16, 0),
                    EndTime = new TimeOnly(20, 0),
                    SlotDurationMinutes = 30
                },

                // Dr. Omar Mahmoud (Orthopedics)
                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "omar.mahmoud@dams.com")).DoctorId,
                    DayOfWeek = "Sunday",
                    StartTime = new TimeOnly(8, 30),
                    EndTime = new TimeOnly(12, 30),
                    SlotDurationMinutes = 30
                },

                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "omar.mahmoud@dams.com")).DoctorId,
                    DayOfWeek = "Thursday",
                    StartTime = new TimeOnly(14, 0),
                    EndTime = new TimeOnly(18, 0),
                    SlotDurationMinutes = 30
                },

                // Dr. Nour El-Din (Dermatology)
                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "nour.eldin@dams.com")).DoctorId,
                    DayOfWeek = "Monday",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(13, 0),
                    SlotDurationMinutes = 45
                },

                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "nour.eldin@dams.com")).DoctorId,
                    DayOfWeek = "Thursday",
                    StartTime = new TimeOnly(15, 0),
                    EndTime = new TimeOnly(18, 0),
                    SlotDurationMinutes = 15
                },

                // Dr. Mariam Adel (OB-GYN)
                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "mariam.adel@dams.com")).DoctorId,
                    DayOfWeek = "Tuesday",
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(13, 0),
                    SlotDurationMinutes = 30
                },

                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "mariam.adel@dams.com")).DoctorId,
                    DayOfWeek = "Thursday",
                    StartTime = new TimeOnly(16, 0),
                    EndTime = new TimeOnly(20, 0),
                    SlotDurationMinutes = 30
                },

                // Dr. Mostafa Ali (Internal Medicine)
                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "mostafa.ali@dams.com")).DoctorId,
                    DayOfWeek = "Sunday",
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(14, 0),
                    SlotDurationMinutes = 30
                },

                new Schedule
                {
                    DoctorId = (await context.Doctors
                        .FirstAsync(d => d.User.Email == "mostafa.ali@dams.com")).DoctorId,
                    DayOfWeek = "Wednesday",
                    StartTime = new TimeOnly(15, 0),
                    EndTime = new TimeOnly(19, 0),
                    SlotDurationMinutes = 30
                }

            );

            await context.SaveChangesAsync();


            // Seed Appointment Slots
            if (!await context.AppointmentSlots.AnyAsync())
            {
                var schedules = await context.Schedules.ToListAsync();

                foreach (var schedule in schedules)
                {
                    GenerateSlotsForSchedule(context, schedule);
                }

                await context.SaveChangesAsync();
            }
        }

    }


    private const int SlotGenerationWeeksAhead = 4;

    private static void GenerateSlotsForSchedule(DamsDbContext context, Schedule schedule)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var targetDayOfWeek = Enum.Parse<DayOfWeek>(schedule.DayOfWeek);

        for (var offset = 0; offset <= SlotGenerationWeeksAhead * 7; offset++)
        {
            var date = today.AddDays(offset);

            if (date.DayOfWeek != targetDayOfWeek)
            {
                continue;
            }

            var slotStart = schedule.StartTime;

            while (slotStart.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
            {
                var slotEnd = slotStart.AddMinutes(schedule.SlotDurationMinutes);

                context.AppointmentSlots.Add(new AppointmentSlot
                {
                    ScheduleId = schedule.ScheduleId,
                    DoctorId = schedule.DoctorId,
                    SlotDate = date.ToDateTime(TimeOnly.MinValue),
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsBooked = false
                });

                slotStart = slotEnd;
            }
        }
    }
}