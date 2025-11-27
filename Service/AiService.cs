using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using queryHelp.Helpers;
using queryHelp.Interfaces;
using queryHelp.Interfaces.AiSmartQueryBuilder.Services;
using queryHelp.Settings;
using static queryHelp.Interfaces.IAiService;

namespace queryHelp.Service
{
    public class AiService : IAiService
    {
        private readonly IHttpClientFactory _http;
        private readonly AiSettings _settings;
        private readonly ISchemaService _schemaService;

        public AiService(IHttpClientFactory http, IOptions<AiSettings> options, ISchemaService schemaService)
        {
            _http = http;
            _settings = options.Value;
            _schemaService = schemaService;
        }

        public async Task<AiSqlResult> GenerateSqlAsync(int datasourceId, string naturalLanguageRequest, CancellationToken ct = default)
        {
            var schema = await _schemaService.GetSchemaSummaryAsync(datasourceId, ct);

            var systemPrompt = @"
You are a helpful assistant that writes SQL Server (T-SQL) SELECT queries only.
You MUST NOT output any destructive statements (INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, EXEC, CREATE).
Output must be valid JSON only with exactly these keys: sql, explain, parameters.
- sql: a parameterized SQL string using @param placeholders.
- explain: a short plain-language explanation of the query and any index hints.
- parameters: an array of { name: string, type: string, example: string }.

If you cannot fulfill the request safely, respond with JSON: { ""sql"": """", ""explain"": ""<reason>"", ""parameters"": [] }.
Do NOT output any additional prose.";

            var userPrompt = $@"
Schema:
{schema}

Request:
{naturalLanguageRequest}

Requirements:
- Use table names and columns only from the Schema above.
- Parameterize filter values using @paramName (no inline literals).
- Keep results under 1000 rows by suggesting TOP or LIMIT if appropriate.
- The response must be valid JSON only (no markdown, no code fences).
- If you need aggregates, group appropriately.
";

            // Create the Chat API payload (OpenAI Chat Completions)
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            };

            var payload = new
            {
                model = _settings.Model,
                messages = messages,
                temperature = _settings.Temperature,
                max_tokens = _settings.MaxTokens,
                n = 1
            };

            var client = _http.CreateClient("openai");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var url = $"{_settings.BaseUrl}/chat/completions";

            var reqContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var resp = await client.PostAsync(url, reqContent, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                // bubble an error (wrap in AiSqlResult)
                return new AiSqlResult(string.Empty, $"AI provider error: {resp.StatusCode} - {raw}", new List<string>(), false, "AI provider error");
            }

            // Parse the response - assume assistant content is JSON
            using var doc = JsonDocument.Parse(raw);
            // Path depends on response format; OpenAI: choices[0].message.content
            string assistantText;
            try
            {
                assistantText = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }
            catch
            {
                assistantText = raw;
            }

            // The assistantText should be a JSON string. Parse it safely.
            string sql = string.Empty;
            string explain = string.Empty;
            List<string> parameters = new();

            try
            {
                // Try to parse assistantText as JSON
                using var jdoc = JsonDocument.Parse(assistantText);
                var root = jdoc.RootElement;
                sql = root.GetProperty("sql").GetString() ?? string.Empty;
                explain = root.GetProperty("explain").GetString() ?? string.Empty;

                if (root.TryGetProperty("parameters", out var pEl) && pEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in pEl.EnumerateArray())
                    {
                        if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("name", out var n))
                        {
                            parameters.Add(n.GetString() ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // if assistant output is not strict JSON, attempt to extract a JSON substring
                // fallback: attempt to find first '{' ... '}' block and parse
                var start = assistantText.IndexOf('{');
                var end = assistantText.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    var jsonSub = assistantText.Substring(start, end - start + 1);
                    try
                    {
                        using var jdoc2 = JsonDocument.Parse(jsonSub);
                        var root = jdoc2.RootElement;
                        sql = root.GetProperty("sql").GetString() ?? string.Empty;
                        explain = root.GetProperty("explain").GetString() ?? string.Empty;
                        if (root.TryGetProperty("parameters", out var pEl) && pEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var el in pEl.EnumerateArray())
                            {
                                if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("name", out var n))
                                {
                                    parameters.Add(n.GetString() ?? "");
                                }
                            }
                        }
                    }
                    catch
                    {
                        // parsing failed
                        return new AiSqlResult(string.Empty, $"Failed to parse model response as JSON. Raw: {assistantText}", new List<string>(), false, "ParseError");
                    }
                }
                else
                {
                    return new AiSqlResult(string.Empty, $"Model did not return JSON. Raw: {assistantText}", new List<string>(), false, "ParseError");
                }
            }

            // Server-side safety check
            if (!SqlSafety.IsLikelySafe(sql, out var reason))
            {
                return new AiSqlResult(sql, explain, parameters, false, reason);
            }

            var paramList = SqlSafety.ExtractParameters(sql);

            return new AiSqlResult(sql, explain, paramList, true, null);
        }
    }
}