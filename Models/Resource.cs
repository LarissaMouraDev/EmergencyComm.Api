using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyComm.Api.Models
{
    public class Resource
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Shelter, Water, Food, Medical, Transport, Communication

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        public bool IsAvailable { get; set; } = true;

        public int? Capacity { get; set; }

        public int? CurrentUsage { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Available"; // Available, Limited, Full, Unavailable

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

        // Contact information
        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [MaxLength(500)]
        public string? AccessInstructions { get; set; }

        // Requirements or restrictions
        [MaxLength(500)]
        public string? Requirements { get; set; }

        [MaxLength(500)]
        public string? Restrictions { get; set; }

        // Resource-specific data (JSON)
        [MaxLength(2000)]
        public string? AdditionalData { get; set; }

        // Navigation properties
        [ForeignKey("ProviderId")]
        public virtual Device? Provider { get; set; }
    }
}