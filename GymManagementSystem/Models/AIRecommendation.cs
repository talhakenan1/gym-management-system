using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagementSystem.Models
{
    public enum RecommendationType
    {
        Exercise,
        Diet,
        Combined
    }

    public enum FitnessGoal
    {
        WeightLoss,
        MuscleGain,
        Endurance,
        Strength,
        GeneralFitness,
        Flexibility
    }

    public enum ActivityLevel
    {
        Sedentary,
        LightlyActive,
        ModeratelyActive,
        VeryActive,
        ExtremelyActive
    }

    public class AIRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public RecommendationType Type { get; set; }

        // User Physical Data
        public int? Age { get; set; }
        public int? Height { get; set; } // cm
        public decimal? Weight { get; set; } // kg
        public string? Gender { get; set; }
        public FitnessGoal? FitnessGoal { get; set; }
        public ActivityLevel? ActivityLevel { get; set; }

        // AI Generated Content
        [Column(TypeName = "nvarchar(max)")]
        public string? ExerciseRecommendations { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? DietRecommendations { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? GeneralAdvice { get; set; }

        public string? PhotoPath { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? PhotoAnalysis { get; set; }

        public string? GeneratedImagePath { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}