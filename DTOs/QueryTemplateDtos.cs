using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.DTOs
{
    public record QueryTemplateCreateDto([Required] string Name, string? Description, [Required] string TemplateSql, string? ParametersJson);
    public record QueryTemplateReadDto(int Id, string Name, string? Description, string TemplateSql, string? ParametersJson, int OwnerId, System.DateTime CreatedAt);
    public record QueryTemplateUpdateDto(string? Name, string? Description, string? TemplateSql, string? ParametersJson);
}
