using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Models
{
    public class User : BaseEntity
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "user"; // user, editor, admin
    }
}