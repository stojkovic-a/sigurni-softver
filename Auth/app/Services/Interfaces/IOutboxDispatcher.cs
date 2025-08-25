namespace Auth.app.Services.Interfaces;

public interface IOutboxDispatcher
{
    Task Ping(Guid messageId);
}
