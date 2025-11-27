using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Interfaces
{
    // File: Services/ISchemaService.cs
    using System.Threading.Tasks;

    namespace AiSmartQueryBuilder.Services
    {
        public interface ISchemaService
        {
            
            Task<string> GetSchemaSummaryAsync(int datasourceId, CancellationToken ct = default);
        }
    }

}