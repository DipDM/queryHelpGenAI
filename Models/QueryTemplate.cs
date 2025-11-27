using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Models
{
    public class QueryTemplate : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string TemplateSql { get; set; } = null!;
        public string? ParametersJson { get; set; }
        public int OwnerId { get; set; }
    }
}