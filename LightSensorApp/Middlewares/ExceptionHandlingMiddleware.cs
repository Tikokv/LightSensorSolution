using System.Net;
using System.Text.Json;

namespace LightSensorApp.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var logDetails = new
            {
                Error = exception.Message,
                StackTrace = exception.StackTrace,
            };

            if (_environment.IsEnvironment("QA"))
            {
                _logger.LogError($"Exception caught: {JsonSerializer.Serialize(logDetails)}");
            }
            else
            {
                _logger.LogError($"Exception caught: {exception.Message}");
            }

            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An error occurred while processing your request. Please try again later."
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
