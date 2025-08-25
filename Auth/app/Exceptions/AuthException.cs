using Microsoft.AspNetCore.Identity;

namespace Auth.app.Exceptions;

public class AuthException : Exception
{
    public class ErrorInfo
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    // public readonly IEnumerable<Tuple<string, string>> errorList;
    public readonly IEnumerable<ErrorInfo> errorList;

    public AuthException(IEnumerable<IdentityError> errors)
    {
        // this.errorList = (IEnumerable<Tuple<string, string>>?)errors.Select(e => new { e.Code, e.Description }) as IEnumerable<Tuple<string, string>> ?? [];
        this.errorList = errors.Select(e => new ErrorInfo { Code = e.Code, Description = e.Description });
        // var test=errors.Select(e => new { e.Code, e.Description });

    }

    public AuthException(string error, int code)
    {
        this.errorList = new List<ErrorInfo>();
        this.errorList.Append(new ErrorInfo()
        {
            Code = code.ToString(),
            Description = error
        });

    }


}
