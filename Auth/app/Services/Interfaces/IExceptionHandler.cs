using Microsoft.AspNetCore.Mvc;

namespace Auth.app.Services;

public interface IExceptionHandler
{
    public ActionResult Handle(Exception ex);
}
