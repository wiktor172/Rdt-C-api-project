namespace WorkExperienceOct2024.Client.Core.Services;

public sealed record ApiResult<T>(T? Data, string? Error)
{
    public bool IsSuccess => Error is null;

    public static ApiResult<T> Success(T data) => new(data, null);

    public static ApiResult<T> Failure(string? error)
        => new(default, string.IsNullOrWhiteSpace(error) ? "An unknown error occurred." : error.Trim());
}
