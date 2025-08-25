using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Auth.app.Context;
using Auth.app.Data.Enums;
using Auth.app.Data.Models;
using Auth.app.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Auth.app.Extensions;

public static class NatsSubscriptionsExtensions
{
    private static async Task<Tuple<Guid, string, NatsMessage>> HandleMessageAsync(IServiceProvider serviceProvider, byte[] message)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var natsMessage = JsonSerializer.Deserialize<NatsMessage>(message);
        string messageIdString = natsMessage.pattern.Split(".")[2];
        Guid messageId = new Guid(messageIdString);
        string userId = natsMessage.pattern.Split(".")[3];
        await context.OutboxMessages
        .Where(m => m.Id == messageId)
        .ExecuteUpdateAsync(m => m.SetProperty(p => p.ProcessedAt, DateTime.Now.ToUniversalTime()));
        await context.SaveChangesAsync();
        return new Tuple<Guid, string, NatsMessage>(messageId, userId, natsMessage);
    }
    public static void NatsSubscriptions(this IApplicationBuilder app)
    {
        var serviceProvider = app.ApplicationServices;
        IPubSubService subService = serviceProvider.GetRequiredService<IPubSubService>();

        subService.SubscribeToTopic($"auth.{Events.STORE_EMAIL.GetDisplayName()}.>", async (sender, message) =>
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            Tuple<Guid, string, NatsMessage> m = await HandleMessageAsync(serviceProvider, message);
            var messageId = m.Item1;
            var userId = m.Item2;
            var natsMessage = m.Item3;
            if (natsMessage.data == "SUCCEEDED")
            {
                Console.WriteLine("User created");
            }
            else
            {
                await authService.RemoveUserAsync(userId);
            }
        });

        subService.SubscribeToTopic($"auth.{Events.CONFIRM_EMAIL.GetDisplayName()}.>", async (sender, message) =>
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            Tuple<Guid, string, NatsMessage> m = await HandleMessageAsync(serviceProvider, message);
            var messageId = m.Item1;
            var userId = m.Item2;
            var natsMessage = m.Item3;

            if (natsMessage.data == "SUCCEEDED")
            {
                Console.WriteLine("User confirmed.");
            }
            else if (natsMessage.data == "NOTFOUND")
            {
                // await authService.RemoveUserAsync(userId);
                // await context.SaveChangesAsync();
                await context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.EmailConfirmed, false));
                await context.SaveChangesAsync();
            }

        });
    }
}
