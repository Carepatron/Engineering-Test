using System;

namespace HerniaSurgical.API.Models
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string? StaffUserId { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public string AppointmentType { get; set; } // "Consultation", "Surgery", "Follow-up", etc.
        public string Status { get; set; } // "Scheduled", "Completed", "Cancelled", "No-Show"
        public string Notes { get; set; }
        public string Provider { get; set; } // Doctor/Surgeon name
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public Client Client { get; set; }
        public User? StaffUser { get; set; }
    }
}