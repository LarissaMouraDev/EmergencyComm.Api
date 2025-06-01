using System.ComponentModel.DataAnnotations;

namespace EmergencyComm.Api.DTOs
{
    public class StatusUpdateDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}