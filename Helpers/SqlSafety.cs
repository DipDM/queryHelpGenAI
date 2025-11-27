using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace queryHelp.Helpers
{
    public static class SqlSafety
    {
        // Reject queries that contain these forbidden keywords (simple check)
        private static readonly string[] Forbidden = new[]
        {
            "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", "EXEC", "MERGE", "CREATE", "GRANT", "REVOKE"
        };

        
        private static readonly Regex ParameterRegex = new Regex(@"(@[A-Za-z0-9_]+)|(:[A-Za-z0-9_]+)", RegexOptions.Compiled);

        public static bool IsLikelySafe(string sql, out string reason)
        {
            reason = string.Empty;
            if (string.IsNullOrWhiteSpace(sql))
            {
                reason = "SQL is empty.";
                return false;
            }

            var up = sql.ToUpperInvariant();

            foreach (var forbidden in Forbidden)
            {
                // allow "WHERE" clauses etc. but block forbidden verbs
                if (Regex.IsMatch(up, $@"\b{Regex.Escape(forbidden)}\b"))
                {
                    reason = $"Contains forbidden keyword: {forbidden}";
                    return false;
                }
            }

            // disallow multiple batches (GO) or semicolons that attempt multiple statements
            if (Regex.IsMatch(up, @"\bGO\b") || up.Count(c => c == ';') > 2)
            {
                reason = "Multiple statements or batch separators detected.";
                return false;
            }

            return true;
        }

        public static List<string> ExtractParameters(string sql)
        {
            var matches = ParameterRegex.Matches(sql);
            return matches.Select(m => m.Value.TrimStart('@', ':')).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}