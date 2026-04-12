using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class RedirectPercentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RedirectPercentMiddleware> _logger;

    public RedirectPercentMiddleware(RequestDelegate next, ILogger<RedirectPercentMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (!string.IsNullOrEmpty(path) && path.EndsWith("%"))
        {
            var safePath = path.TrimEnd('%');
            _logger.LogWarning($"Redirecting invalid URL '{path}' to '{safePath}'");
            context.Response.Redirect(safePath);
            return;
        }

        await _next(context);
    }
}