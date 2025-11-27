using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.DTOs
{
    public record DataSourceCreateDto([Required] string Name, [Required] string ConnectionString, bool IsReadOnly = true);
    public record DataSourceReadDto(int Id, string Name, bool IsReadOnly, int OwnerId, System.DateTime CreatedAt);
    public record DataSourceUpdateDto(string? Name, string? ConnectionString, bool? IsReadOnly);
}