// File: Service/SchemaService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using queryHelp.Data;
using queryHelp.Interfaces.AiSmartQueryBuilder.Services;

namespace queryHelp.Service
{
    public class SchemaService : ISchemaService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SchemaService> _logger;
        private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(30);

        public SchemaService(
            ApplicationDbContext db,
            IMemoryCache cache,
            ILogger<SchemaService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> GetSchemaSummaryAsync(int datasourceId, CancellationToken ct = default)
        {
            var cacheKey = $"schema:{datasourceId}";
            if (_cache.TryGetValue<string>(cacheKey, out var cached) && !string.IsNullOrWhiteSpace(cached))
            {
                _logger.LogDebug("SchemaService: returning cached schema for datasource {Id}", datasourceId);
                return cached;
            }

            var ds = await _db.DataSources.AsNoTracking().FirstOrDefaultAsync(d => d.Id == datasourceId, ct);
            if (ds == null)
            {
                _logger.LogWarning("SchemaService: datasource {Id} not found", datasourceId);
                return string.Empty;
            }

            var connString = DecryptConnectionString(ds.EncryptedConnectionString);
            if (string.IsNullOrWhiteSpace(connString))
            {
                _logger.LogWarning("SchemaService: datasource {Id} has empty or invalid connection string", datasourceId);
                return string.Empty;
            }

            try
            {
                var tableColumns = new Dictionary<string, List<(string Column, string Type)>>();

                using var conn = new SqlConnection(connString);
                await conn.OpenAsync(ct);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA NOT IN ('sys','INFORMATION_SCHEMA')
ORDER BY TABLE_NAME, ORDINAL_POSITION";
                cmd.CommandTimeout = 30;

                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    var schema = reader.GetString(0);
                    var table = reader.GetString(1);
                    var column = reader.GetString(2);
                    var type = reader.GetString(3);

                    var key = $"{schema}.{table}";
                    if (!tableColumns.TryGetValue(key, out var list))
                    {
                        list = new List<(string, string)>();
                        tableColumns[key] = list;
                    }
                    list.Add((column, type));
                }

               
                var sb = new StringBuilder();
                sb.AppendLine("// Schema discovered from target database (limited to first 40 tables)");

                int maxTables = 40;
                int i = 0;
                foreach (var kv in tableColumns)
                {
                    if (i++ >= maxTables) break;
                    sb.Append($"{kv.Key}: ");
                    sb.Append(string.Join(", ", kv.Value.Select(c => $"{c.Column}:{c.Type}")));
                    sb.AppendLine();
                }

                var summary = sb.ToString().Trim();
                _cache.Set(cacheKey, summary, _cacheTtl);

                _logger.LogInformation("SchemaService: discovered {TableCount} tables for datasource {Id}", tableColumns.Count, datasourceId);
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SchemaService: failed reading schema for datasource {Id}", datasourceId);
                return string.Empty;
            }
        }

        private string DecryptConnectionString(string encrypted)
        {
            if (string.IsNullOrWhiteSpace(encrypted)) return string.Empty;

           
            try
            {
               
                var maybePlain = Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
                
                if (maybePlain.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
                    maybePlain.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
                {
                    return maybePlain;
                }
            }
            catch
            {
               
            }

            
            return encrypted;
        }
    }
}
