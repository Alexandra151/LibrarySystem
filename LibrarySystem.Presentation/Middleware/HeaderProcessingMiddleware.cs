using System.Diagnostics;

namespace LibrarySystem.Presentation.Middleware
{
    public class HeaderProcessingMiddleware
    {
        private readonly RequestDelegate _next;
        private const string RequestIdHeader = "X-Request-Id";
        private const string ClientNameHeader = "X-Client-Name";

        public HeaderProcessingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ClientNameHeader, out var clientName) ||
                string.IsNullOrWhiteSpace(clientName))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"Missing required header: {ClientNameHeader}");
                return;
            }

            if (!context.Request.Headers.TryGetValue(RequestIdHeader, out var rid) ||
                string.IsNullOrWhiteSpace(rid))
            {
                var newId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
                context.Request.Headers[RequestIdHeader] = newId;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[RequestIdHeader] =
                    context.Request.Headers[RequestIdHeader].ToString();
                context.Response.Headers[ClientNameHeader] =
                    context.Request.Headers[ClientNameHeader].ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }

    }

    public static class HeaderProcessingExtensions
    {
        public static IApplicationBuilder UseHeaderProcessing(this IApplicationBuilder app)
            => app.UseMiddleware<HeaderProcessingMiddleware>();
    }
}
