using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Auth.app.Services.Interfaces;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;

namespace Auth.app.Services;

public class AuthorizationServiceCustom : AuthorizationService.AuthorizationServiceBase
{
    private readonly TokenValidationParameters _tokenValParams;
    public AuthorizationServiceCustom(TokenValidationParameters tokenValParams)
    {
        _tokenValParams = tokenValParams;
    }
    public override Task<TestReply> TestFunction(TestMessage request, ServerCallContext context)
    {
        var replt = new TestReply();
        replt.OtherGield.AddRange(new[] { 4, 5, 6 });
        return Task.FromResult(replt);
    }
    public override Task<TokenValidationReply> ValidateToken(TokenRequest request, ServerCallContext context)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(request.Token, _tokenValParams, out SecurityToken validatedToken);

            var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);

            return Task.FromResult(new TokenValidationReply
            {
                IsValid = true,
                Claims = { claims }
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TokenValidationReply
            {
                IsValid = false,
                Error = ex.Message
            });
        }
    }
}
