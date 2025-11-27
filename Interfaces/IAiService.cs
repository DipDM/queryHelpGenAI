using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Interfaces
{
    public interface IAiService
    {
        Task<AiSqlResult> GenerateSqlAsync(int datasourceId, string naturalLanguageRequest, CancellationToken ct = default);
    }

    public record AiSqlResult(string Sql, string Explain, List<string> Parameters, bool IsSafe, string? SafetyReason);
}
