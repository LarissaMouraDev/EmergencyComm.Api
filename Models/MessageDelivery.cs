using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyComm.Api.Models
{
    public class MessageDelivery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MessageId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecipientId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Delivered, Read, Failed

        public DateTime? DeliveredAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RetryCount { get; set; } = 0;

        public int MaxRetries { get; set; } = 3;

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        // Navigation properties
        [ForeignKey("MessageId")]
        public virtual Message? Message { get; set; }

        [ForeignKey("RecipientId")]
        public virtual Device? Recipient { get; set; }
    }
}