using Auth.app.Services;

public static class RoleSeederExtensions
{
    public static async Task SeedRolesAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await authService.SeedRolesAsync();
    }
}
