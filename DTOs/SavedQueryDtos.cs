using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.DTOs
{
    public record SavedQueryCreateDto([Required] string Name, int? TemplateId, [Required] string SqlText, string? ParametersJson, string? SharedWith);
    public record SavedQueryReadDto(int Id, string Name, int? TemplateId, string SqlText, string? ParametersJson, string? SharedWith, int OwnerId, System.DateTime CreatedAt);
    public record SavedQueryUpdateDto(string? Name, string? SqlText, string? ParametersJson, string? SharedWith);
}