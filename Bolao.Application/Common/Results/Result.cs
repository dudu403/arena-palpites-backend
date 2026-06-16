namespace Bolao.Application.Common.Results;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    public static Result<T> Success(T value, int statusCode = 200)
    {
        return new Result<T>(true, value, null, statusCode);
    }

    public static Result<T> Failure(string error, int statusCode)
    {
        return new Result<T>(false, default, error, statusCode);
    }
}