using System.Text.Json.Serialization;

namespace TestManagement.Client.Models.Common;

public class ODataListResult<T>
{
    public List<T> Items { get; set; } = new();
    public int? Count { get; set; }
}

public class ODataResponse<T>
{
    [JsonPropertyName("@odata.count")]
    public int? Count { get; set; }

    public List<T> Value { get; set; } = new();
}
