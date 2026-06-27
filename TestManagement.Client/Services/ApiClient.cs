using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TestManagement.Client.Configuration;
using TestManagement.Client.Models.Common;

namespace TestManagement.Client.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResult<T>> GetAsync<T>(string endpoint)
    {
        using var request = CreateRequest(HttpMethod.Get, endpoint);
        return await SendAsync<T>(request);
    }

    public async Task<ApiResult<ODataListResult<T>>> GetODataListAsync<T>(string endpoint)
    {
        using var request = CreateRequest(HttpMethod.Get, endpoint);
        using var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ClearTokenIfUnauthorized(response.StatusCode);
            return ApiResult<ODataListResult<T>>.Fail(ExtractError(content), (int)response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return ApiResult<ODataListResult<T>>.Ok(new ODataListResult<T>());
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                var items = JsonSerializer.Deserialize<List<T>>(content, _jsonOptions) ?? new List<T>();
                return ApiResult<ODataListResult<T>>.Ok(new ODataListResult<T> { Items = items });
            }

            var wrapped = JsonSerializer.Deserialize<ODataResponse<T>>(content, _jsonOptions) ?? new ODataResponse<T>();
            return ApiResult<ODataListResult<T>>.Ok(new ODataListResult<T>
            {
                Items = wrapped.Value,
                Count = wrapped.Count
            });
        }
        catch (JsonException)
        {
            return ApiResult<ODataListResult<T>>.Fail("Không thể đọc dữ liệu từ API.", (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        using var request = CreateJsonRequest(HttpMethod.Post, endpoint, data);
        return await SendAsync<TResponse>(request);
    }

    public async Task<ApiResult<string>> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        using var request = CreateJsonRequest(HttpMethod.Post, endpoint, data);
        return await SendStringAsync(request);
    }

    public async Task<ApiResult<string>> PostEmptyAsync(string endpoint)
    {
        using var request = CreateRequest(HttpMethod.Post, endpoint);
        return await SendStringAsync(request);
    }

    public async Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        using var request = CreateJsonRequest(HttpMethod.Put, endpoint, data);
        return await SendAsync<TResponse>(request);
    }

    public async Task<ApiResult<string>> PutAsync<TRequest>(string endpoint, TRequest data)
    {
        using var request = CreateJsonRequest(HttpMethod.Put, endpoint, data);
        return await SendStringAsync(request);
    }

    public async Task<ApiResult<string>> PatchAsync<TRequest>(string endpoint, TRequest data)
    {
        using var request = CreateJsonRequest(HttpMethod.Patch, endpoint, data);
        return await SendStringAsync(request);
    }

    public async Task<ApiResult<string>> DeleteAsync(string endpoint)
    {
        using var request = CreateRequest(HttpMethod.Delete, endpoint);
        return await SendStringAsync(request);
    }

    private HttpRequestMessage CreateJsonRequest<TRequest>(HttpMethod method, string endpoint, TRequest data)
    {
        var request = CreateRequest(method, endpoint);
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return request;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, endpoint);
        var token = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.AccessToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request)
    {
        using var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            ClearTokenIfUnauthorized(response.StatusCode);
            return ApiResult<T>.Fail(ExtractError(content), (int)response.StatusCode);
        }

        if (typeof(T) == typeof(string))
        {
            return ApiResult<T>.Ok((T)(object)content, (int)response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return ApiResult<T>.Ok(default, (int)response.StatusCode);
        }

        try
        {
            var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
            return ApiResult<T>.Ok(data, (int)response.StatusCode);
        }
        catch (JsonException)
        {
            return ApiResult<T>.Fail("Không thể đọc dữ liệu từ API.", (int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResult<string>> SendStringAsync(HttpRequestMessage request)
    {
        using var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return ApiResult<string>.Ok(content, (int)response.StatusCode);
        }

        ClearTokenIfUnauthorized(response.StatusCode);
        return ApiResult<string>.Fail(ExtractError(content), (int)response.StatusCode);
    }

    private void ClearTokenIfUnauthorized(HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.Unauthorized)
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SessionKeys.AccessToken);
        }
    }

    private string ExtractError(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "Yêu cầu không thành công.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind == JsonValueKind.String)
            {
                return document.RootElement.GetString() ?? "Yêu cầu không thành công.";
            }

            if (document.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? "Dữ liệu không hợp lệ.";
            }

            if (document.RootElement.TryGetProperty("errors", out var errors))
            {
                return errors.ToString();
            }
        }
        catch (JsonException)
        {
            return content.Trim('"');
        }

        return content;
    }
}
