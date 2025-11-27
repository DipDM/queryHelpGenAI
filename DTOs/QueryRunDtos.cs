using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.DTOs
{
    public record QueryRunRequestDto(int? SavedQueryId, string? SqlText, string? ParametersJson, int DatasourceId);
    public record QueryRunReadDto(int Id, int? SavedQueryId, int UserId, string? ParametersJson, System.DateTime StartedAt, System.DateTime? FinishedAt, int RowsReturned, string Status, string? ErrorMessage);
}