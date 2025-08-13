using Microsoft.EntityFrameworkCore;
using HerniaSurgical.API.Data;
using HerniaSurgical.API.Models;
using HerniaSurgical.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=herniasurgical.db"));

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseCors("AllowAll");

app.MapPost("/api/conversations", async (CreateConversationDto dto, AppDbContext db) =>
{
    try
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            PatientName = dto.PatientName,
            StartedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            CreatedByUserId = dto.CreatedByUserId,
            CreatedByUserName = dto.CreatedByUserName,
            CreatedByUserRole = dto.CreatedByUserRole
        };

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            conversation.Id,
            conversation.PatientName,
            conversation.StartedAt,
            conversation.LastMessageAt,
            MessageCount = 0
        });
    }
    catch
    {
        Console.WriteLine("Error creating conversation");
        return Results.BadRequest("Failed to create conversation");
    }
});

app.MapGet("/api/conversations", async (HttpContext httpContext, AppDbContext db) =>
{
    try
    {
        var userId = httpContext.Request.Query["userId"].ToString();
        var userRole = httpContext.Request.Query["userRole"].ToString();
        
        Console.WriteLine($"API Request - userId: '{userId}', userRole: '{userRole}'");
        
        var conversationsQueryable = db.Conversations.Include(c => c.Messages).AsQueryable();
        
        // Filter conversations based on user role
        if (!string.IsNullOrEmpty(userId) && userRole == "Patient")
        {
            Console.WriteLine($"Filtering conversations for patient: {userId}");
            // Patients can only see conversations they created or where they sent messages
            conversationsQueryable = conversationsQueryable.Where(c => 
                c.CreatedByUserId == userId || 
                c.Messages.Any(m => m.SenderUserId == userId));
        }
        else
        {
            Console.WriteLine($"No filtering - userId empty: {string.IsNullOrEmpty(userId)}, userRole: '{userRole}'");
        }
        // Staff members can see all conversations (no filtering)

        var conversations = await conversationsQueryable
            .Select(c => new
            {
                c.Id,
                c.PatientName,
                c.StartedAt,
                c.LastMessageAt,
                LastMessage = c.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault().Content,
                MessageCount = c.Messages.Count,
                c.CreatedByUserId,
                c.CreatedByUserName,
                c.CreatedByUserRole
            })
            .ToListAsync();

        return Results.Ok(conversations);
    }
    catch
    {
        Console.WriteLine("Error getting conversations");
        return Results.Ok(new List<object>());
    }
});

app.MapGet("/api/conversations/{id}/messages", async (Guid id, AppDbContext db) =>
{
    var conversation = await db.Conversations
        .Include(c => c.Messages)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (conversation == null)
        return Results.NotFound();

    var messages = conversation.Messages
        .OrderBy(m => m.Timestamp)
        .Select(m => new
        {
            m.Id,
            m.Content,
            m.IsFromPatient,
            m.Timestamp,
            m.SenderUserId,
            m.SenderUserName,
            m.SenderUserRole
        });

    return Results.Ok(new
    {
        conversation.Id,
        conversation.PatientName,
        Messages = messages
    });
});

app.MapPost("/api/conversations/{id}/messages", async (Guid id, MessageDto messageDto, AppDbContext db, IHubContext<ConversationHub> hubContext) =>
{
    var conversation = await db.Conversations.FindAsync(id);
    if (conversation == null)
        return Results.NotFound();

    var message = new Message
    {
        Id = Guid.NewGuid(),
        ConversationId = id,
        Content = messageDto.Content,
        IsFromPatient = messageDto.SenderUserRole == "Patient",
        Timestamp = DateTime.UtcNow,
        SenderUserId = messageDto.SenderUserId,
        SenderUserName = messageDto.SenderUserName,
        SenderUserRole = messageDto.SenderUserRole
    };

    db.Messages.Add(message);
    conversation.LastMessageAt = message.Timestamp;
    
    await db.SaveChangesAsync();
    
    var messageResponse = new
    {
        message.Id,
        message.Content,
        message.IsFromPatient,
        message.Timestamp,
        message.SenderUserId,
        message.SenderUserName,
        message.SenderUserRole
    };

    try
    {
        await hubContext.Clients.All.SendAsync("ReceiveMessage", id, messageResponse);
        Console.WriteLine($"Sent patient message via SignalR: {message.Content}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send patient message via SignalR: {ex.Message}");
    }

    var aiResponse = GenerateAIResponse(messageDto.Content);
    if (!string.IsNullOrEmpty(aiResponse))
    {
        var aiMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = id,
            Content = aiResponse,
            IsFromPatient = false,
            Timestamp = DateTime.UtcNow.AddSeconds(1),
            SenderUserId = "ai-system",
            SenderUserName = "AI Assistant",
            SenderUserRole = "System"
        };

        db.Messages.Add(aiMessage);
        conversation.LastMessageAt = aiMessage.Timestamp;
        await db.SaveChangesAsync();
        
        var aiMessageResponse = new
        {
            aiMessage.Id,
            aiMessage.Content,
            aiMessage.IsFromPatient,
            aiMessage.Timestamp,
            aiMessage.SenderUserId,
            aiMessage.SenderUserName,
            aiMessage.SenderUserRole
        };
        
        try
        {
            await hubContext.Clients.All.SendAsync("ReceiveMessage", id, aiMessageResponse);
            Console.WriteLine($"Sent AI response via SignalR: {aiResponse}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send AI response via SignalR: {ex.Message}");
        }
    }

    return Results.Ok(messageResponse);
});

// Client endpoints
app.MapGet("/api/clients", async (AppDbContext db) =>
{
    try
    {
        var clients = await db.Clients
            .Include(c => c.Appointments)
            .Include(c => c.Conversations)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Phone,
                c.DateOfBirth,
                c.InsuranceProvider,
                c.InsurancePolicyNumber,
                c.CreatedAt,
                c.UpdatedAt,
                AppointmentCount = c.Appointments.Count,
                ConversationCount = c.Conversations.Count
            })
            .ToListAsync();
        return Results.Ok(clients);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting clients: {ex.Message}");
        return Results.Problem("Failed to retrieve clients");
    }
});

app.MapGet("/api/clients/{id}", async (Guid id, AppDbContext db) =>
{
    try
    {
        var client = await db.Clients
            .Include(c => c.Appointments)
            .Include(c => c.Conversations)
            .FirstOrDefaultAsync(c => c.Id == id);
        
        if (client == null)
            return Results.NotFound($"Client with ID {id} not found");
        
        return Results.Ok(client);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting client: {ex.Message}");
        return Results.Problem("Failed to retrieve client");
    }
});

app.MapPost("/api/clients", async (ClientDto clientDto, AppDbContext db) =>
{
    try
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            FirstName = clientDto.FirstName,
            LastName = clientDto.LastName,
            Email = clientDto.Email,
            Phone = clientDto.Phone,
            DateOfBirth = clientDto.DateOfBirth,
            InsuranceProvider = clientDto.InsuranceProvider,
            InsurancePolicyNumber = clientDto.InsurancePolicyNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        
        return Results.Created($"/api/clients/{client.Id}", client);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating client: {ex.Message}");
        return Results.Problem("Failed to create client");
    }
});

app.MapPut("/api/clients/{id}", async (Guid id, ClientDto clientDto, AppDbContext db) =>
{
    try
    {
        var client = await db.Clients.FindAsync(id);
        
        if (client == null)
            return Results.NotFound($"Client with ID {id} not found");
        
        client.FirstName = clientDto.FirstName;
        client.LastName = clientDto.LastName;
        client.Email = clientDto.Email;
        client.Phone = clientDto.Phone;
        client.DateOfBirth = clientDto.DateOfBirth;
        client.InsuranceProvider = clientDto.InsuranceProvider;
        client.InsurancePolicyNumber = clientDto.InsurancePolicyNumber;
        client.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return Results.Ok(client);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating client: {ex.Message}");
        return Results.Problem("Failed to update client");
    }
});

// Appointment endpoints
app.MapGet("/api/appointments", async (AppDbContext db) =>
{
    try
    {
        var appointments = await db.Appointments
            .Include(a => a.Client)
            .Select(a => new
            {
                a.Id,
                a.ClientId,
                ClientName = a.Client.FullName,
                a.AppointmentDate,
                a.AppointmentType,
                a.Status,
                a.Provider,
                a.DurationMinutes,
                a.Notes,
                a.CreatedAt,
                a.UpdatedAt
            })
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();
        
        return Results.Ok(appointments);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting appointments: {ex.Message}");
        return Results.Problem("Failed to retrieve appointments");
    }
});

app.MapGet("/api/appointments/{id}", async (Guid id, AppDbContext db) =>
{
    try
    {
        var appointment = await db.Appointments
            .Include(a => a.Client)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (appointment == null)
            return Results.NotFound($"Appointment with ID {id} not found");
        
        return Results.Ok(appointment);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting appointment: {ex.Message}");
        return Results.Problem("Failed to retrieve appointment");
    }
});

app.MapGet("/api/clients/{clientId}/appointments", async (Guid clientId, AppDbContext db) =>
{
    try
    {
        var appointments = await db.Appointments
            .Where(a => a.ClientId == clientId)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();
        
        return Results.Ok(appointments);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting client appointments: {ex.Message}");
        return Results.Problem("Failed to retrieve appointments");
    }
});

app.MapPost("/api/appointments", async (AppointmentDto appointmentDto, AppDbContext db) =>
{
    try
    {
        // Verify client exists
        var clientExists = await db.Clients.AnyAsync(c => c.Id == appointmentDto.ClientId);
        if (!clientExists)
            return Results.BadRequest($"Client with ID {appointmentDto.ClientId} does not exist");
        
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            ClientId = appointmentDto.ClientId,
            AppointmentDate = appointmentDto.AppointmentDate,
            AppointmentType = appointmentDto.AppointmentType,
            Status = appointmentDto.Status ?? "Scheduled",
            Provider = appointmentDto.Provider,
            DurationMinutes = appointmentDto.DurationMinutes,
            Notes = appointmentDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();
        
        return Results.Created($"/api/appointments/{appointment.Id}", appointment);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating appointment: {ex.Message}");
        return Results.Problem("Failed to create appointment");
    }
});

app.MapPut("/api/appointments/{id}", async (Guid id, AppointmentDto appointmentDto, AppDbContext db) =>
{
    try
    {
        var appointment = await db.Appointments.FindAsync(id);
        
        if (appointment == null)
            return Results.NotFound($"Appointment with ID {id} not found");
        
        // Verify client exists if changing client
        if (appointmentDto.ClientId != appointment.ClientId)
        {
            var clientExists = await db.Clients.AnyAsync(c => c.Id == appointmentDto.ClientId);
            if (!clientExists)
                return Results.BadRequest($"Client with ID {appointmentDto.ClientId} does not exist");
        }
        
        appointment.ClientId = appointmentDto.ClientId;
        appointment.AppointmentDate = appointmentDto.AppointmentDate;
        appointment.AppointmentType = appointmentDto.AppointmentType;
        appointment.Status = appointmentDto.Status ?? appointment.Status;
        appointment.Provider = appointmentDto.Provider;
        appointment.DurationMinutes = appointmentDto.DurationMinutes;
        appointment.Notes = appointmentDto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
        
        return Results.Ok(appointment);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating appointment: {ex.Message}");
        return Results.Problem("Failed to update appointment");
    }
});

app.MapHub<ConversationHub>("/conversationHub");

app.Run();

string GenerateAIResponse(string message)
{
    message = message.ToLower();
    
    if (message.Contains("schedule") || message.Contains("appointment"))
        return "Please call us at 555-0123 to schedule an appointment.";
    else if (message.Contains("reschedule"))
        return "To reschedule, please call our office at 555-0123.";
    else if (message.Contains("price") || message.Contains("cost") || message.Contains("how much"))
        return "Pricing varies based on insurance. Please contact us for details.";
    else if (message.Contains("hernia"))
        return "A hernia occurs when an organ pushes through an opening in the muscle. Our specialists can help.";
    else if (message.Contains("insurance"))
        return "We accept most major insurance plans. Please call to verify coverage.";
    else if (message.Contains("hello") || message.Contains("hi"))
        return "Hello! How can I help you today?";
    else
        return "Thank you for your message. Our team will get back to you soon.";
}

public class MessageDto
{
    public string Content { get; set; }
    public string? SenderUserId { get; set; }
    public string? SenderUserName { get; set; }
    public string? SenderUserRole { get; set; }
}

public class CreateConversationDto
{
    public string PatientName { get; set; }
    public Guid? ClientId { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? CreatedByUserRole { get; set; }
}

public class ClientDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string InsuranceProvider { get; set; }
    public string InsurancePolicyNumber { get; set; }
}

public class AppointmentDto
{
    public Guid ClientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentType { get; set; }
    public string Status { get; set; }
    public string Provider { get; set; }
    public int DurationMinutes { get; set; }
    public string Notes { get; set; }
}