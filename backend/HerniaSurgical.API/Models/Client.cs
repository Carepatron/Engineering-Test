using System;
using System.Collections.Generic;

namespace HerniaSurgical.API.Models
{
    public class Client
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string InsuranceProvider { get; set; }
        public string InsurancePolicyNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
        
        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
}