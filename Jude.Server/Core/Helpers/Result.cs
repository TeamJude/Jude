namespace Jude.Server.Core.Helpers;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Message { get; protected set; }
    public int? ErrorCode { get; protected set; }
    public List<string>? Errors { get; protected set; }

    protected Result(
        bool isSuccess,
        string? message = null,
        int? errorCode = null,
        List<string>? errors = null
    )
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
        Errors = errors;
    }

    public static Result Ok(string? message = null) => new(true, message);

    public static Result Fail(string message, int code = 400) => new(false, null, code, [message]);

    public static Result Fail(List<string> errors, int code = 400) =>
        new(false, null, code, errors);

    public static Result Exception(string message) => new(false, null, 500, [message]);
}

public class ResultWithData<T> : Result
{
    public T? Data { get; }

    private ResultWithData(
        bool isSuccess,
        T? data,
        string? message = null,
        int? errorCode = null,
        List<string>? errors = null
    )
        : base(isSuccess, message, errorCode, errors)
    {
        Data = data;
    }

    public static ResultWithData<T> Ok(T data, string? message = null) => new(true, data, message);

    public static new ResultWithData<T> Fail(List<string> errors, int code = 400) =>
        new(false, default, null, code, errors);

    public static new ResultWithData<T> Exception(string errorMessage) =>
        new(false, default, null, 500, [errorMessage]);
}
