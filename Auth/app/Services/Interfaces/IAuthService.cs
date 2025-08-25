using System.Formats.Asn1;
using Auth.app.Data.DTOs;

namespace Auth.app.Services;

public interface IAuthService
{
    public Task<string> RegisterAsync(RegisterUser registerUser);
    public Task RemoveUserAsync(string Id);
    public Task SeedRolesAsync();

    public Task<TokenPair> LogInAsync(LoginUser loginUser);
    public Task<TokenPair> LogInWithRefreshTokenAsync(string refreshToken);

    public Task<bool> RevokeRefreshTokensAsync(string userId);

    public Task ConfirmEmailAsync(string userId, string emailCode);
    public void Test();
}
