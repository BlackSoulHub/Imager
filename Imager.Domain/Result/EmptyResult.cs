namespace Imager.Domain.Result;

public class EmptyResult<TError>
{
    private readonly TError? _error;

    public EmptyResult()
    {
        IsFailure = false;
    }

    public EmptyResult(TError error)
    {
        IsFailure = true;
        _error = error;
    }

    public bool IsFailure { get; }

    public TError UnwrapError() => _error ?? throw new NullReferenceException("Ошибка пуста");

    public static EmptyResult<TError> Ok() => new();
    public static EmptyResult<TError> WithError(TError error) => new(error);
}