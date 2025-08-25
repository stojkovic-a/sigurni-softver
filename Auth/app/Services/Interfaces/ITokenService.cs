using Auth.app.Data.Enums;
using Auth.app.Data.Models;

namespace Auth.app.Services;

public interface ITokenService
{
    public string Create(User user, string[] roles);

    public string GenerateRefreshToken();
    public string GenerateEmailConfirmationToken();

}
