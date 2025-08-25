
using Microsoft.AspNetCore.Identity;

namespace Auth.app.Data.Models;

public class User : IdentityUser
{
    // public long Id { get; set; }
    public string Name { get; set; } = String.Empty;
    // public string UserName { get; set; } = String.Empty;
    // public string PasswordHash { get; set; } = String.Empty;
    // public string Email { get; set; } = String.Empty;
    // public string ProfileId { get; set; } = String.Empty;//Maybe remove 
    // public string ProfilePhotoUrl { get; set; } = String.Empty;
    // public string Status { get; set; } = String.Empty;
    // public bool AccountConfirmed { get; set; }
    // public bool IsVisible { get; set; }
    // public bool IsModerator { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime CreationTime { get; set; }
}
