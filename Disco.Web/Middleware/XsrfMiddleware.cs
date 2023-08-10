using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Disco.Web.Models;

namespace Disco.Web.Middleware;

public class XsrfMiddleware
{
    private readonly RequestDelegate _next;

    public XsrfMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private string GetExpectedValue(HttpContext context, string expectedToken)
    {
        // calculate expected hash
        var unixNow = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        // round to hour
        unixNow = unixNow - (unixNow % 3600);
        var expectedHash = Convert.ToBase64String(
            SHA256.Create().ComputeHash(
                Encoding.UTF8.GetBytes(
                    expectedToken + context.Request.Path + context.Request.Method + context.Request.Headers.UserAgent + unixNow
                )
            )
        );
        return expectedHash;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip XSRF check for bot api.
        if (context.Request.Path.ToString().StartsWith("/api/Bot/"))
        {
            await _next(context);
            return;
        }
        if (context.Request.Method is not "GET" or "OPTIONS")
        {
            context.Request.Headers.TryGetValue("xsrf", out var providedToken);

            var expectedValue = "";
            var isMatch = false;
            if (context.Request.Cookies.TryGetValue("xsrf", out var expectedToken) && !string.IsNullOrWhiteSpace(providedToken))
            {
                expectedValue = expectedToken;
                // compare provided hash
                var expectedHash = GetExpectedValue(context, expectedToken);
                isMatch = providedToken == expectedHash;
            }

            if (string.IsNullOrWhiteSpace(expectedValue) || string.IsNullOrWhiteSpace(providedToken) || !isMatch)
            {
                var newId = Guid.NewGuid().ToString();
                context.Response.Cookies.Append("xsrf", newId, new CookieOptions()
                {
                    Path = "/",
                    HttpOnly = true,
                    IsEssential = true,
                    MaxAge = TimeSpan.FromMinutes(30),
                    SameSite = SameSiteMode.Lax,
                });
                context.Response.StatusCode = 403;
                context.Response.Headers["xsrf"] = GetExpectedValue(context, newId);
                await context.Response.WriteAsJsonAsync(new HttpError()
                {
                    code = "Xsrf",
                    message = "Invalid or missing XSRF token"
                });
                return;
            }
        }
        await _next(context);
    }
}

public static class XsrfMiddlewareExtensions
{
    public static IApplicationBuilder UseXsrfMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<XsrfMiddleware>();
    }
}