
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Auth.app.Context;
using Auth.app.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.app.Services;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _provider;
    public OutboxPublisher(IServiceProvider provider)
    {
        _provider = provider;
    }

    // public async Task Ping(Guid messageId)
    // {
    //     using var scope = _provider.CreateScope();
    //     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //     var pubService = scope.ServiceProvider.GetRequiredService<IPubSubService>();

    //     var message = await dbContext.OutboxMessages.FindAsync(messageId);
    //     if (message == null) return;
    //     this.SendMessage(dbContext, pubService, message);
    // }
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var pubService = scope.ServiceProvider.GetRequiredService<IPubSubService>();

            var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccuredAt)
            .Take(10)
            .ToListAsync();

            if (messages.Count() != 0)
            {
                foreach (var message in messages)
                {
                    SendMessage(dbContext, pubService, message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
