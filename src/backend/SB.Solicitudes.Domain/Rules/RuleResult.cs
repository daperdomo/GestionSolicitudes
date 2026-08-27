namespace SB.Solicitudes.Domain.Rules;

public readonly record struct RuleResult(bool IsSuccess, string Code, string Error)
{
    public static RuleResult Success() => new(true, string.Empty, string.Empty);

    public static RuleResult Failure(string code, string error) => new(false, code, error);
}
