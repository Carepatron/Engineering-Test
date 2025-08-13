using HerniaSurgical.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HerniaSurgical.API.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if users already exist
            if (await context.Users.AnyAsync())
            {
                return; // Database already seeded
            }

            // Seed users from FAKE_USERS in AuthContext.tsx
            var users = new List<User>
            {
                new User
                {
                    Id = "1",
                    Name = "Sarah Johnson",
                    Email = "sarah@gmail.com",
                    Role = "Medical Assistant",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = "2", 
                    Name = "John Smith",
                    Email = "john.smith@email.com",
                    Role = "Patient",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = "ai-system",
                    Name = "AI Assistant", 
                    Email = "ai@system.com",
                    Role = "System",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Seed clients (some linked to patient users)
            var clients = new List<Client>
            {
                new Client
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@email.com", // Same email as user ID "2"
                    Phone = "555-0123",
                    DateOfBirth = new DateTime(1985, 6, 15),
                    InsuranceProvider = "Blue Cross Blue Shield",
                    InsurancePolicyNumber = "BCBS123456789",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Emily",
                    LastName = "Davis",
                    Email = "emily.davis@email.com",
                    Phone = "555-0234",
                    DateOfBirth = new DateTime(1992, 3, 22),
                    InsuranceProvider = "Aetna",
                    InsurancePolicyNumber = "AET987654321",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Client
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Michael",
                    LastName = "Brown",
                    Email = "michael.brown@email.com",
                    Phone = "555-0345",
                    DateOfBirth = new DateTime(1978, 11, 8),
                    InsuranceProvider = "Cigna",
                    InsurancePolicyNumber = "CIG456789123",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();

            // Seed appointments linking clients and staff
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = Guid.NewGuid(),
                    ClientId = clients[0].Id, // John Smith
                    StaffUserId = "1", // Sarah Johnson (Medical Assistant)
                    StartDateUtc = DateTime.UtcNow.AddDays(1).Date.AddHours(9), // Next week at 9 AM
                    EndDateUtc = DateTime.UtcNow.AddDays(1).Date.AddHours(10), // Next week at 10 AM
                    AppointmentType = "Initial Consultation",
                    Status = "Scheduled",
                    Provider = "Dr. Sarah Johnson",
                    Notes = "Patient requesting consultation for hernia repair options",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Appointment
                {
                    Id = Guid.NewGuid(),
                    ClientId = clients[1].Id, // Emily Davis
                    StaffUserId = "1", // Sarah Johnson (Medical Assistant)
                    StartDateUtc = DateTime.UtcNow.AddDays(0).Date.AddHours(14), // 10 days from now at 2 PM
                    EndDateUtc = DateTime.UtcNow.AddDays(0).Date.AddHours(15), // 10 days from now at 3 PM
                    AppointmentType = "Follow-up",
                    Status = "Scheduled",
                    Provider = "Dr. Sarah Johnson",
                    Notes = "Post-surgery follow-up appointment",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Appointment
                {
                    Id = Guid.NewGuid(),
                    ClientId = clients[2].Id, // Michael Brown
                    StaffUserId = "1", // Sarah Johnson (Medical Assistant)
                    StartDateUtc = DateTime.UtcNow.AddDays(3).Date.AddHours(11), // 3 days from now at 11 AM
                    EndDateUtc = DateTime.UtcNow.AddDays(3).Date.AddHours(12), // 3 days from now at 12 PM
                    AppointmentType = "Pre-surgical Assessment",
                    Status = "Scheduled",
                    Provider = "Dr. Sarah Johnson",
                    Notes = "Pre-operative assessment and preparation",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Appointments.AddRangeAsync(appointments);
            await context.SaveChangesAsync();

            // Seed some conversations linking users
            var conversations = new List<Conversation>
            {
                new Conversation
                {
                    Id = Guid.NewGuid(),
                    PatientName = "John Smith - General Inquiry",
                    ClientId = clients[0].Id, // Link to John Smith client
                    StartedAt = DateTime.UtcNow.AddDays(-2),
                    LastMessageAt = DateTime.UtcNow.AddDays(-1),
                    CreatedByUserId = "2", // Created by John Smith user
                    CreatedByUserName = "John Smith",
                    CreatedByUserRole = "Patient"
                },
                new Conversation
                {
                    Id = Guid.NewGuid(),
                    PatientName = "Emily Davis - Post-Surgery Questions",
                    ClientId = clients[1].Id, // Link to Emily Davis client
                    StartedAt = DateTime.UtcNow.AddDays(-5),
                    LastMessageAt = DateTime.UtcNow.AddDays(-4),
                    CreatedByUserId = "1", // Created by Sarah Johnson (staff)
                    CreatedByUserName = "Sarah Johnson",
                    CreatedByUserRole = "Medical Assistant"
                }
            };

            await context.Conversations.AddRangeAsync(conversations);
            await context.SaveChangesAsync();

            // Seed schedule for Sarah Johnson (Medical Assistant)
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                StaffUserId = "1", // Sarah Johnson
                Name = "Default Schedule",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Schedules.AddAsync(schedule);
            await context.SaveChangesAsync();

            // Seed schedule slots for Sarah Johnson's weekly schedule (Mon-Fri 9am-5pm)
            var scheduleSlots = new List<ScheduleSlot>
            {
                // Monday: 9am - 5pm
                new ScheduleSlot
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = schedule.Id,
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                    EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                    IsAvailable = true,
                    Notes = "Monday 9am-5pm"
                },
                // Tuesday: 9am - 5pm
                new ScheduleSlot
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = schedule.Id,
                    DayOfWeek = DayOfWeek.Tuesday,
                    StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                    EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                    IsAvailable = true,
                    Notes = "Tuesday 9am-5pm"
                },
                // Wednesday: 9am - 5pm
                new ScheduleSlot
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = schedule.Id,
                    DayOfWeek = DayOfWeek.Wednesday,
                    StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                    EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                    IsAvailable = true,
                    Notes = "Wednesday 9am-5pm"
                },
                // Thursday: 9am - 5pm
                new ScheduleSlot
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = schedule.Id,
                    DayOfWeek = DayOfWeek.Thursday,
                    StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                    EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                    IsAvailable = true,
                    Notes = "Thursday 9am-5pm"
                },
                // Friday: 9am - 5pm
                new ScheduleSlot
                {
                    Id = Guid.NewGuid(),
                    ScheduleId = schedule.Id,
                    DayOfWeek = DayOfWeek.Friday,
                    StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                    EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                    IsAvailable = true,
                    Notes = "Friday 9am-5pm"
                }
                // Saturday and Sunday: Off (no slots)
            };

            await context.ScheduleSlots.AddRangeAsync(scheduleSlots);
            await context.SaveChangesAsync();

            Console.WriteLine("Database seeded with comprehensive data:");
            Console.WriteLine($"- {users.Count} users");
            Console.WriteLine($"- {clients.Count} clients");
            Console.WriteLine($"- {appointments.Count} appointments");
            Console.WriteLine($"- {conversations.Count} conversations");
            Console.WriteLine($"- 1 schedule with {scheduleSlots.Count} time slots");
            Console.WriteLine("All entities are properly linked with foreign keys");
        }
    }
}