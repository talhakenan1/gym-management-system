using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementSystem.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int Duration { get; set; } // Duration in minutes

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = "Genel";

        public bool IsActive { get; set; } = true;

        // Foreign Key
        public int GymId { get; set; }

        // Navigation properties
        public virtual Gym Gym { get; set; } = null!;
        public virtual ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}