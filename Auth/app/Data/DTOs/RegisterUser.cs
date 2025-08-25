using Auth.app.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.app.Data.DTOs;

public class RegisterUser
{
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
}
