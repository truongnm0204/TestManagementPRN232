using System.Text;

namespace TestManagement.Client.Services;

public static class ODataQueryBuilder
{
    public static string Build(string endpoint, string? filter, string? orderBy, int page, int pageSize, bool includeCount = true)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var parameters = new List<string>
        {
            $"$top={safePageSize}",
            $"$skip={(safePage - 1) * safePageSize}"
        };

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            parameters.Add($"$orderby={Uri.EscapeDataString(orderBy)}");
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            parameters.Add($"$filter={Uri.EscapeDataString(filter)}");
        }

        if (includeCount)
        {
            parameters.Add("$count=true");
        }

        return $"{endpoint}?{string.Join('&', parameters)}";
    }

    public static string EscapeValue(string value)
    {
        return value.Trim().Replace("'", "''");
    }

    public static string Contains(string field, string value)
    {
        return $"contains(tolower({field}),'{EscapeValue(value).ToLowerInvariant()}')";
    }

    public static string And(params string?[] filters)
    {
        return Join(" and ", filters);
    }

    public static string Or(params string?[] filters)
    {
        return Join(" or ", filters);
    }

    private static string Join(string separator, params string?[] filters)
    {
        var validFilters = filters.Where(filter => !string.IsNullOrWhiteSpace(filter)).ToList();
        if (validFilters.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        for (var i = 0; i < validFilters.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(separator);
            }

            builder.Append('(').Append(validFilters[i]).Append(')');
        }

        return builder.ToString();
    }
}
