using Auth.app.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Auth.app.Services;

public class ExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<Exception, ActionResult>> _handlers;
    public ExceptionHandler()
    {
        _handlers = new Dictionary<Type, Func<Exception, ActionResult>>
            {
                { typeof(UnauthorizedAccessException), ex => new UnauthorizedResult() },
                { typeof(ArgumentNullException), ex => new BadRequestObjectResult($"Missing argument: {ex.Message}") },
                { typeof(ArgumentException), ex => new BadRequestObjectResult($"Invalid argument: {ex.Message}") },
                { typeof(KeyNotFoundException), ex => new NotFoundObjectResult($"Not found: {ex.Message}") },
                { typeof(InvalidOperationException), ex => new BadRequestObjectResult($"Invalid operation: {ex.Message}") },
                {typeof(AuthException),ex=> new BadRequestObjectResult(new {message="Registration failed", errors=((AuthException)ex).errorList}) }
                // Add more application-specific exceptions as needed
        };
    }
    public ActionResult Handle(Exception ex)
    {
        if (_handlers.TryGetValue(ex.GetType(), out var handler))
        {
            return handler(ex);
        }

        return new ObjectResult($"An error has occured: {ex.Message}")
        {
            StatusCode = 500
        };
    }
}
