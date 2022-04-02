namespace PeterPedia.Server.Services;

public abstract class Result<T>
{
    private T? _data;

    protected Result(T? data) => _data = data;

    public bool Success { get; protected set; }

    public bool Failure => !Success;

    public T Data
    {
        get => Success ? _data ?? throw new Exception($"{nameof(Data)} must be set for sucess results.") : throw new Exception($"You can't access .{nameof(Data)} when .{nameof(Success)} is false");
        set => _data = value;
    }
}

public class SuccessResult<T> : Result<T> where T : notnull
{
    public SuccessResult(T data) : base(data) => Success = true;
}

public class ErrorResult<T> : Result<T>
{
    public ErrorResult(string message) : base(default)
    {
        Message = message;
        Success = false;
    }

    public string Message { get; set; }
}

public class NotFoundResult<T> : Result<T>
{
    public NotFoundResult() : base(default) => Success = false;
}

public class ConflictResult<T> : Result<T>
{
    public ConflictResult() : base(default) => Success = false;
}
