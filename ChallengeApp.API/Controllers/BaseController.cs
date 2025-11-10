using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeApp.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        protected string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        protected string GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}