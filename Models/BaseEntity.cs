using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Models
{

    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
