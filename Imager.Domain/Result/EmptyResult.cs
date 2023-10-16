namespace Imager.Domain.Result;

public class EmptyResult<TError>
{
    private readonly TError? _error;

    public EmptyResult()
    {
       
    }

    public EmptyResult(TError error)
    {
        _error = error;
    }

    public bool IsError() => _error is not null;
    public TError UnwrapError => _error ?? throw new NullReferenceException("Ошибка пуста");

    public static EmptyResult<TError> Ok() => new();
    public static EmptyResult<TError> WithError(TError error) => new(error);
}