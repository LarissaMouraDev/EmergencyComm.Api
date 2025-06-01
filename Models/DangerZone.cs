using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.Models
{
    public class DangerZone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Fire, Flood, Earthquake, Chemical, Violence, Building_Collapse

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public double RadiusMeters { get; set; } = 100;

        [Required]
        [MaxLength(20)]
        public string SeverityLevel { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Required]
        [MaxLength(50)]
        public string ReportedBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsVerified { get; set; } = false;

        public int ConfirmationCount { get; set; } = 1;

        [MaxLength(500)]
        public string? SafetyInstructions { get; set; }

        [MaxLength(500)]
        public string? EvacuationRoute { get; set; }

        [MaxLength(100)]
        public string? EmergencyContact { get; set; }

        // Additional zone data (JSON for polygons, etc.)
        [MaxLength(2000)]
        public string? GeometryData { get; set; }

        [MaxLength(500)]
        public string? AdditionalInfo { get; set; }

        // Status tracking
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Resolved, Under_Control, Escalating
    }
}