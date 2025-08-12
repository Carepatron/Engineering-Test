using System;
using System.Collections.Generic;

namespace HerniaSurgical.API.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public List<Conversation> CreatedConversations { get; set; } = new List<Conversation>();
        public List<Message> SentMessages { get; set; } = new List<Message>();
    }
}