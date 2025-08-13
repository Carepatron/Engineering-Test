using System;

namespace HerniaSurgical.API.Models
{
    public class ScheduleSlot
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; } // Monday = 1, Tuesday = 2, etc.
        public TimeSpan StartTime { get; set; } // e.g., 09:00:00
        public TimeSpan EndTime { get; set; } // e.g., 13:00:00
        public bool IsAvailable { get; set; } = true; // Can be used for temporary unavailability
        public string? Notes { get; set; } // Optional notes like "Lunch break 12-1"
        
        // Navigation property
        public Schedule Schedule { get; set; }
    }
}