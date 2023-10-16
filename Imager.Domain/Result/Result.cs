namespace Imager.Domain.Result;

public class Result<TResult, TError>
{
    private readonly TResult? _result;
    private readonly TError? _error;

    private Result(TResult result)
    {
        _result = result;
    }

    private Result(TError error)
    {
        _error = error;
    }

    public bool IsError => _error is not null;
    public TResult Unwrap() => _result ?? throw new NullReferenceException("Контент отсутствует");
    public TError UnwrapError() => _error ?? throw new NullReferenceException("Ошибка пуста");
    
    public static Result<TResult, TError> Ok(TResult result) => new(result);
    public static Result<TResult, TError> WithError(TError error) => new(error);
}