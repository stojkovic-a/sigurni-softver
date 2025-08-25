using Auth.app.Context;
using Microsoft.EntityFrameworkCore;

namespace Auth.app.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        //Nece idk sto
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using Context.AppDbContext context = scope.ServiceProvider.GetRequiredService<Context.AppDbContext>();

        context.Database.Migrate();
    }
}
