namespace TestManagement.BAL.DTOs.Common;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static ServiceResult Ok()
    {
        return new ServiceResult { Success = true };
    }

    public static ServiceResult Fail(string error)
    {
        return new ServiceResult { Success = false, Error = error };
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data)
    {
        return new ServiceResult<T> { Success = true, Data = data };
    }

    public new static ServiceResult<T> Fail(string error)
    {
        return new ServiceResult<T> { Success = false, Error = error };
    }
}
