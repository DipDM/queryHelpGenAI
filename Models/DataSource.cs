using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Models
{
    public class DataSource : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string EncryptedConnectionString { get; set; } = null!;
        public int OwnerId { get; set; }
        public bool IsReadOnly { get; set; } = true;

    }
}