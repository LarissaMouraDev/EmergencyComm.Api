using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.DTOs
{
    public class CreateResourceDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProviderId { get; set; } = string.Empty;

        public int? Capacity { get; set; }

        public int? CurrentUsage { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [MaxLength(500)]
        public string? AccessInstructions { get; set; }

        [MaxLength(500)]
        public string? Requirements { get; set; }

        [MaxLength(500)]
        public string? Restrictions { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [MaxLength(2000)]
        public string? AdditionalData { get; set; }
    }
}