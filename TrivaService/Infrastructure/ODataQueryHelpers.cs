using System.Text.RegularExpressions;

namespace TrivaService.Infrastructure
{
    public static class ODataQueryHelpers
    {
        public static string ExtractSearchTerm(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return string.Empty;

            var match = Regex.Match(filter, "'([^']*)'");
            return match.Success ? match.Groups[1].Value.Replace("''", "'").Trim() : filter.Trim();
        }

        // Extracts value from: contains(FieldName,'value')
        public static string ExtractFieldFilter(string? filter, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(filter)) return string.Empty;
            var pattern = $@"contains\(\s*{Regex.Escape(fieldName)}\s*,\s*'([^']*)'\)";
            var match = Regex.Match(filter, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Replace("''", "'").Trim() : string.Empty;
        }

        // Extracts value from: FieldName eq value  or  FieldName eq 'value'
        public static string ExtractEqFilter(string? filter, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(filter)) return string.Empty;
            var pattern = $@"\b{Regex.Escape(fieldName)}\s+eq\s+(?:'([^']*)'|([^\s)&|]+))";
            var match = Regex.Match(filter, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) return string.Empty;
            return match.Groups[1].Success
                ? match.Groups[1].Value.Replace("''", "'").Trim()
                : match.Groups[2].Value.Trim();
        }

        public static IEnumerable<T> ApplyPagination<T>(IEnumerable<T> source, int? skip, int? top)
        {
            var query = source;
            if (skip.HasValue && skip.Value > 0)
                query = query.Skip(skip.Value);
            if (top.HasValue && top.Value > 0)
                query = query.Take(top.Value);
            return query;
        }

        public static object ToLookupResult(int id, string text)
        {
            return new { id, text };
        }
    }
}
