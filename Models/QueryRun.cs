using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Models
{
    public class QueryRun : BaseEntity
    {
        public int? SavedQueryId { get; set; }
        public int UserId { get; set; }
        public string? ParametersJson { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
        public int RowsReturned { get; set; }
        public string Status { get; set; } = "Pending"; 
        public string? ErrorMessage { get; set; }
    }
}