using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.DTOs
{
    public class CreateMessageDto
    {
        [Required]
        [MaxLength(50)]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(50)]
        public string MessageType { get; set; } = "Text";

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        public bool IsEmergency { get; set; } = false;

        public bool IsBroadcast { get; set; } = false;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public List<string>? RecipientIds { get; set; }

        public int MaxHops { get; set; } = 5;
    }
}