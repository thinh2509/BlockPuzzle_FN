using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class ScoreDto
    {
        [Required]
        public int Score { get; set; }
    }
}
