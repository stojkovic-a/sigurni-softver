namespace Auth.app.Data.Models;

public class EmailConfirmationOutboxMessage 
{
    public string UserId { get; set; } = string.Empty;
    public string ConfirmationCode { get; set; } = string.Empty;
}
