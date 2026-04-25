using System.Text.RegularExpressions;

namespace TrivaService.Infrastructure
{
    public static class ODataQueryHelpers
    {
        public static string ExtractSearchTerm(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return string.Empty;
            }

            var match = Regex.Match(filter, "'([^']*)'");
            return match.Success ? match.Groups[1].Value.Replace("''", "'").Trim() : filter.Trim();
        }

        public static IEnumerable<T> ApplyPagination<T>(IEnumerable<T> source, int? skip, int? top)
        {
            var query = source;
            if (skip.HasValue && skip.Value > 0)
            {
                query = query.Skip(skip.Value);
            }

            if (top.HasValue && top.Value > 0)
            {
                query = query.Take(top.Value);
            }

            return query;
        }

        public static object ToLookupResult(int id, string text)
        {
            return new { id, text };
        }
    }
}
