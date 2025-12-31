using CleanTemplate.Application.Common.Models;

namespace CleanTemplate.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
