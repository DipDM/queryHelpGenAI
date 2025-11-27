using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Models
{
    public class AuditLog : BaseEntity
    {
        public int? UserId { get; set; }
        [Required]
        public string Action { get; set; } = null!;
        public string? DetailsJson { get; set; }
    }
}