
using System.Text;
using System.Text.Json;
using Auth.app.Context;
using Auth.app.Data.Models;

namespace Auth.app.Services.Interfaces;

public class OutboxDispatcher : IOutboxDispatcher
{
    private readonly IServiceProvider _provider;
    public OutboxDispatcher(IServiceProvider provider)
    {
        _provider = provider;

    }
    public async Task Ping(Guid messageId)
    {
        try
        {
            using var scope = _provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pubService = scope.ServiceProvider.GetRequiredService<IPubSubService>();

            var message = await dbContext.OutboxMessages.FindAsync(messageId);
            if (message == null) return;
            this.SendMessage(dbContext, pubService, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString(), $"Something went wrong with publishing the outbox message {messageId}");
        }
    }
    private void SendMessage(AppDbContext dbContext, IPubSubService pubService, OutboxMessage message)
    {
        try
        {
            pubService.SendMessage($"cache.{message.Event}", Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message)));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString(), $"Failed to publish outbox message {message.Id}");
        }
    }
}
