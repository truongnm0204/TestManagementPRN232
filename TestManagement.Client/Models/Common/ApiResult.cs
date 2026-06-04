using Microsoft.AspNetCore.Http;

namespace TestManagement.Client.Models.Common;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public int StatusCode { get; set; }

    public static ApiResult<T> Ok(T? data, int statusCode = StatusCodes.Status200OK)
    {
        return new ApiResult<T> { Success = true, Data = data, StatusCode = statusCode };
    }

    public static ApiResult<T> Fail(string? error, int statusCode)
    {
        return new ApiResult<T> { Success = false, Error = error, StatusCode = statusCode };
    }
}
