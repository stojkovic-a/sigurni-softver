using System.Security.Cryptography.X509Certificates;
using System.Text;
using Auth.app.Context;
using Auth.app.Data.Models;
using Auth.app.Extensions;
using Auth.app.Services;
using Auth.app.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme =
    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Description = "Enter JWT Bearer token **_only_**",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = "Bearer",
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };
    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {securityScheme,Array.Empty<string>()}
    });
});


var tokenValidationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["JWT:Issuer"],
    ValidAudience = builder.Configuration["JWT:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),
    ClockSkew = TimeSpan.Zero
};
builder.Services.AddSingleton(tokenValidationParams);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = tokenValidationParams;
})
.AddBearerToken(IdentityConstants.BearerScheme, options =>
{
    options.BearerTokenExpiration = TimeSpan.FromMinutes(10);
});

builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<Auth.app.Context.AppDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

builder.Services.AddDbContext<Auth.app.Context.AppDbContext>(options =>
options.UseNpgsql(builder.Configuration["PostgresClientAuth:connectionString"]));

builder.Services.AddGrpc();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IExceptionHandler, ExceptionHandler>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IPubSubService, NatsPubSubService>();
builder.Services.AddSingleton<IOutboxDispatcher, OutboxDispatcher>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHostedService<OutboxPublisher>();


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7147, listenOptions =>
    {
        listenOptions.UseHttps(builder.Configuration.GetValue<string>("Certs:GrpcsServerPfx")!, builder.Configuration["Cert:exportPass"], httpsOptions =>
        {
            httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
            httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
            {
                var caCert = new X509Certificate2(builder.Configuration.GetValue<string>("Certs:ClientCA")!);
                return chain.Build(cert) && chain.ChainElements[^1].Certificate.Thumbprint == caCert.Thumbprint;
            };
        });
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    options.ListenLocalhost(5253, ListenOptions =>
    {
        ListenOptions.UseHttps();
        ListenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // app.ApplyMigrations();
}
else
{
}
await app.SeedRolesAsync();
app.NatsSubscriptions();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
// app.MapIdentityApi<User>();

app.MapControllers();
app.MapGrpcService<AuthorizationServiceCustom>();
app.Run();


