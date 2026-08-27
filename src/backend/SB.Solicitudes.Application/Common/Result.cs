using System.Diagnostics.CodeAnalysis;

namespace SB.Solicitudes.Application.Common;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Forbidden = 3,
    Conflict = 4,
    Unauthorized = 5,
}

public sealed record OperationError(ErrorType Type, string Code, string Message);

public sealed class Result<T>
{
    private Result(T? value, OperationError? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public OperationError? Error { get; }
    public bool IsSuccess => Error is null;

    [SuppressMessage("Design", "CA1000", Justification = "Named result factories keep use-case return paths explicit.")]
    public static Result<T> Success(T value) => new(value, null);

    [SuppressMessage("Design", "CA1000", Justification = "Named result factories keep use-case return paths explicit.")]
    public static Result<T> Failure(ErrorType type, string code, string message) =>
        new(default, new OperationError(type, code, message));
}
