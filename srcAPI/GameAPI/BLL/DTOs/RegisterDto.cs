using System.ComponentModel.DataAnnotations;
using System;

namespace WebAPI.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public string? Name { get; set; }
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }
}
