using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Models
{
    public class SavedQuery : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;
        public int? TemplateId { get; set; }
        public string SqlText { get; set; } = null!;
        public string? ParametersJson { get; set; }
        public string? SharedWith { get; set; } 
        public int OwnerId { get; set; }
    }
}