using System;
using System.Collections.Generic;

namespace HerniaSurgical.API.Models
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public string PatientName { get; set; }
        public Guid? ClientId { get; set; } // Optional link to Client
        public DateTime StartedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        
        // User tracking
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? CreatedByUserRole { get; set; }
        
        // Navigation properties
        public Client Client { get; set; }
        public User? CreatedByUser { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}