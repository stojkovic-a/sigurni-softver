using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Auth.app.Context;
using Auth.app.Data.DTOs;
using Auth.app.Data.Enums;
using Auth.app.Data.Models;
using Auth.app.Exceptions;
using Auth.app.Services.Interfaces;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;

namespace Auth.app.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IPubSubService _pubSub;
    private readonly IOutboxDispatcher _dispatcher;
    private readonly AppDbContext _dbContext;

    public AuthService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<User> signInManager,
        IConfiguration config,
        IEmailService emailService,
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor,
        IPubSubService pubSub,
        IOutboxDispatcher dispatcher,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _config = config;
        _emailService = emailService;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _pubSub = pubSub;
        _dispatcher = dispatcher;
        _httpContext = httpContextAccessor;
    }


    public void Test()
    {
        //TODO::Remove the function and IPubSub dependency!!!!!!
        _pubSub.SendMessage("cache.test", Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { message = "test" })));
    }
    public async Task<TokenPair> LogInAsync(LoginUser loginUser)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(loginUser.UserName);
            if (user == null || !user.EmailConfirmed)
            {
                throw new AuthException("Username or Password wrong.", 400);
            }
            var passwordValid = await _userManager.CheckPasswordAsync(user, loginUser.Password);
            if (!passwordValid)
            {
                throw new AuthException("Username or Password wrong.", 400);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.IsNullOrEmpty())
            {
                throw new AuthException("Username or Password wrong", 400);
            }
            string accessToken = _tokenService.Create(user, roles.ToArray());
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Id = Guid.NewGuid(),
                Token = _tokenService.GenerateRefreshToken(),
                ExpiresOnUtc = DateTime.UtcNow.AddDays(_config.GetValue<int>("Identity:RefreshToken:ExpirationInDays"))
            };
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new TokenPair()
            {
                RefreshToken = refreshToken.Token,
                AccessToken = accessToken
            };
        }
        catch
        {
            throw;
        }
    }

    public async Task<TokenPair> LogInWithRefreshTokenAsync(string refreshTokenOld)
    {
        try
        {
            RefreshToken? refreshToken = await _dbContext.RefreshTokens
            .Include(r => r.User)
            .OrderByDescending(r => r.ExpiresOnUtc)
            .FirstOrDefaultAsync(r => r.Token == refreshTokenOld);

            if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
            {
                throw new ApplicationException("The refresh token has expird");
            }
            var roles = await _userManager.GetRolesAsync(refreshToken.User);
            if (roles.IsNullOrEmpty())
            {
                throw new ApplicationException("Something went wrong, try again later");
            }
            string accessToken = _tokenService.Create(refreshToken.User, roles.ToArray());
            refreshToken.Token = _tokenService.GenerateRefreshToken();
            refreshToken.ExpiresOnUtc = DateTime.UtcNow.AddDays(_config.GetValue<int>("Identity:RefreshToken:ExpirationInDays"));
            await _dbContext.SaveChangesAsync();

            return new TokenPair()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }
        catch
        {
            throw;
        }

    }

    public async Task<string> RegisterAsync(RegisterUser registerUser)
    {
        ///
        /// Creates user, adds user role to user, sends email via emailService,
        /// adds confirmation link key to redis
        ///
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                UserName = registerUser.UserName,
                Email = registerUser.Email,
                CreationTime = DateTime.Now.ToUniversalTime(),
                Name = registerUser.Name,
                DateOfBirth = registerUser.DateOfBirth
            };
            var userCreationResult = await _userManager.CreateAsync(user, registerUser.Password);
            if (!userCreationResult.Succeeded)
            {
                throw new AuthException(userCreationResult.Errors);
            }

            var roleAddAsync = await _userManager.AddToRoleAsync(user, Enum.GetName(Roles.USER) ?? "USER");
            if (!roleAddAsync.Succeeded)
            {
                throw new AuthException(roleAddAsync.Errors);
            }

            // var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // if (emailConfirmationToken == null)
            // {
            //     throw new AuthException("Email Confirmation Token failed to generate.", 400);
            // }
            var emailConfirmationToken = _tokenService.GenerateEmailConfirmationToken();
            var messageId = Guid.NewGuid();
            await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = messageId,
                Event = Events.STORE_EMAIL.GetDisplayName(),
                Payload = JsonSerializer.Serialize(new EmailConfirmationOutboxMessage()
                {
                    ConfirmationCode = emailConfirmationToken,
                    UserId = user.Id
                }),
                Succeeded = null,
                OccuredAt = DateTime.Now.ToUniversalTime(),
                ProcessedAt = null
            });
            await _dbContext.SaveChangesAsync();


            // // var result = await _userManager.SetAuthenticationTokenAsync(user, "backend", "EmailConfirmation", emailConfirmationToken);
            // // if (!result.Succeeded)
            // // {
            // //     throw new AuthException(result.Errors);
            // // }


            await transaction.CommitAsync();
            // await _dispatcher.Ping(messageId);
            _ = Task.Run(async () =>
            {
                await _dispatcher.Ping(messageId);
            });
            //send emailConfirmationToken via email
            //cache emailConfirmationToken in Redis for expiry
            return user.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    public async Task RemoveUserAsync(string Id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return;
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new AuthException(result.Errors);
            }
        }
        catch
        {
            throw;
        }

    }

    public async Task<bool> RevokeRefreshTokensAsync(string userId)
    {
        try
        {
            if (userId != GetCurrentUserId())
            {
                throw new ApplicationException("You can't do this");
            }
            await _dbContext.RefreshTokens
            .Where(r => r.UserId == userId)
            .ExecuteDeleteAsync();
            return true;
        }
        catch
        {
            throw;
        }

    }

    private string? GetCurrentUserId()
    {
        return _httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task SeedRolesAsync()
    {
        string[] roles = Enum.GetNames(typeof(Roles));

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public async Task ConfirmEmailAsync(string userId, string emailCode)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new AuthException("User not found.", 404);
            }
            var messageId = Guid.NewGuid();
            await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
            {
                Id = messageId,
                Event = Events.CONFIRM_EMAIL.GetDisplayName(),
                Payload = JsonSerializer.Serialize(new CreateProfileOutboxMessage()
                {
                    ConfirmationCode = emailCode,
                    DateOfBirth = user.DateOfBirth,
                    Name = user.Name,
                    UserId = userId,
                    UserName = user.UserName

                }),
                Succeeded = null,
                OccuredAt = DateTime.Now.ToUniversalTime(),
                ProcessedAt = null
            });
            await _dbContext.SaveChangesAsync();
            _ = Task.Run(async () =>
            {
                await _dispatcher.Ping(messageId);
            });
            // // var emailCodeDb = await _userManager.GetAuthenticationTokenAsync(user, "backend", "EmailConfirmation");
            // // if (emailCodeDb == null)
            // // {
            // //     throw new AuthException("Resend mail for email confirmation.", 404);
            // // }

            // // var codeValid = await _userManager.VerifyUserTokenAsync(user, "backend", "EmailConfirmation", emailCode);
            // // if (!codeValid)
            // // {
            // //     throw new AuthException("Email verification failed.", 400);
            // // }
            await _dbContext.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.EmailConfirmed, true));
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
