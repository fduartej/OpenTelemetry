using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace ReFactoring.Middleware;
public class LoggingContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingContextMiddleware> _logger;

    public LoggingContextMiddleware(RequestDelegate next, ILogger<LoggingContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Leer CustomerId desde encabezado o query
        var customerId = context.Request.Headers["X-Customer-Id"].FirstOrDefault()
                         ?? context.Request.Query["customerId"].FirstOrDefault()
                         ?? "anonymous";

        // Añadir como tag a la actividad (para OpenTelemetry tracing)
        Activity.Current?.AddTag("CustomerId", customerId);

        // Usar BeginScope para agregarlo al contexto de logs
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CustomerId"] = customerId
        }))
        {
            await _next(context); // continuar el pipeline
        }
    }
}
