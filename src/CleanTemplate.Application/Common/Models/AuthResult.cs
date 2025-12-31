namespace CleanTemplate.Application.Common.Models;

public sealed record AuthResult(bool Succeeded, string? Token, IReadOnlyList<string> Errors)
{
    public static AuthResult Success(string token) => new(true, token, []);

    public static AuthResult Failure(IEnumerable<string> errors) => new(false, null, [.. errors]);
}
