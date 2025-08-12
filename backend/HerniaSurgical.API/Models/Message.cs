using System;

namespace HerniaSurgical.API.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string Content { get; set; }
        public bool IsFromPatient { get; set; }
        public DateTime Timestamp { get; set; }
        
        // User tracking
        public string? SenderUserId { get; set; }
        public string? SenderUserName { get; set; }
        public string? SenderUserRole { get; set; }
        
        // Navigation properties
        public Conversation Conversation { get; set; }
        public User? SenderUser { get; set; }
    }
}