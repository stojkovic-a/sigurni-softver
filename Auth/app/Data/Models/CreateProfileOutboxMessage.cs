namespace Auth.app.Data.Models;

public class CreateProfileOutboxMessage
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;

}
