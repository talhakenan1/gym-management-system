namespace GymManagementSystem.Models
{
    public class TrainerService
    {
        public int Id { get; set; }

        // Foreign Keys
        public int TrainerId { get; set; }
        public int ServiceId { get; set; }

        // Navigation properties
        public virtual Trainer Trainer { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}