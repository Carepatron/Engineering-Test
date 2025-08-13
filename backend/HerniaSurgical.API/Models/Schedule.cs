using System;
using System.Collections.Generic;

namespace HerniaSurgical.API.Models
{
    public class Schedule
    {
        public Guid Id { get; set; }
        public string StaffUserId { get; set; }
        public string Name { get; set; } // e.g., "Default Schedule", "Summer Schedule"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public User StaffUser { get; set; }
        public List<ScheduleSlot> Slots { get; set; } = new List<ScheduleSlot>();
    }
}