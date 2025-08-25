using System.Formats.Asn1;
using Auth.app.Data.DTOs;
using Auth.app.Data.Models;
using Auth.app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Auth.app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{

    private readonly IAuthService _authService;
    private readonly IExceptionHandler _exceptionHandler;
    public AuthController(
        IAuthService authService,
        IExceptionHandler exceptionHandler)
    {
        _authService = authService;
        _exceptionHandler = exceptionHandler;
    }

    [Authorize]
    [HttpGet("testAuthController")]
    public ActionResult Test()
    {
        // _authService.Test();
        return Ok();
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUser registerUser)
    {
        try
        {
            var emailCode = await _authService.RegisterAsync(registerUser);
            return Ok(emailCode);
        }
        catch (Exception ex)
        {
            return _exceptionHandler.Handle(ex);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginUser user)
    {
        try
        {
            TokenPair tokenPair = await _authService.LogInAsync(user);
            return Ok(tokenPair);
        }
        catch (Exception ex)
        {
            return _exceptionHandler.Handle(ex);
        }
    }

    [HttpPost("loginWithRefreshToken")]
    public async Task<ActionResult> LoginWithRefreshToken([FromBody] string refreshToken)
    {
        try
        {
            TokenPair tokenPair = await _authService.LogInWithRefreshTokenAsync(refreshToken);
            return Ok(tokenPair);
        }
        catch (Exception ex)
        {
            return _exceptionHandler.Handle(ex);
        }
    }
    [HttpDelete("revokeRefreshToken")]
    public async Task<ActionResult> RevokeRefreshToken([FromBody] string userId)
    {
        try
        {
            var result = await _authService.RevokeRefreshTokensAsync(userId);
            return Ok("Refresh tokens revoked.");
        }
        catch (Exception ex)
        {
            return _exceptionHandler.Handle(ex);
        }
    }

    [HttpGet("confirmEmail")]
    public async Task<ActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string emailCode)
    {
        try
        {
            await _authService.ConfirmEmailAsync(userId, emailCode);
            return Ok("Email successfully confirmed.");
        }
        catch (Exception ex)
        {
            return _exceptionHandler.Handle(ex);
        }
    }

    [HttpDelete("deleteUser/{id}")]
    public async Task<ActionResult> DeleteUser([FromRoute] string id)
    {
        try
        {
            await _authService.RemoveUserAsync(id);
            return Ok("User removed");
        }
        catch (Exception ex)
        {
            return _exceptionHandler.Handle(ex);
        }
    }
    // [HttpPost("refresh")]
    // public async Task<ActionResult> Refresh([FromBody] string refreshToken)
    // {

    // }

    // [HttpGet("confirmEmail")]
    // public async Task<ActionResult> ConfirmEmail([FromQuery] string userEmail, [FromQuery] string confirmationCode)
    // {


    // }

    // [HttpPost("forgotPassword")]
    // public async Task<ActionResult> ForgotPassword([FromBody] string email)
    // {

    // }

    // [HttpPost("resetPassword")]
    // public async Task<ActionResult> ResetPassword([FromBody] string email, [FromBody] string resetCode, [FromBody] string newPassword)
    // {

    // }
}
