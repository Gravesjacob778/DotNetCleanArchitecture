namespace CleanTemplate.WebApi.Models.Auth;

public sealed record AuthResponse(string? Token, IReadOnlyList<string> Errors);
