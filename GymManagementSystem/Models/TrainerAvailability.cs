using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        // Foreign Key
        public int TrainerId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual Trainer Trainer { get; set; } = null!;
    }
}