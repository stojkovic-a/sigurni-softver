using Auth.app.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.app.Context;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    // public DbSet<CreateProfileOutboxMessage> CreateProfileOutboxMessages { get; set; }
    // public DbSet<EmailConfirmationOutboxMessage> EmailConfirmationOutboxMessages { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.HasDefaultSchema("identity");
    }
}
