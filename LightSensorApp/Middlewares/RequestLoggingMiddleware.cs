using Microsoft.AspNetCore.Http.Extensions;
using System.Diagnostics;
using System.Text;

namespace LightSensorApp.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await LogRequestDetails(context);

            await _next(context);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseContent = await GetResponseBody(responseBodyStream);

            await LogResponseDetails(context, stopwatch, responseContent);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;
        }

        private async Task LogRequestDetails(HttpContext context)
        {
            string ipAddress = context.Connection.RemoteIpAddress?.ToString();
            string targetUrl = context.Request.GetDisplayUrl();
            string requestBody = await GetRequestBody(context.Request);

            _logger.LogInformation($"Request from IP Address: {ipAddress}, Target URL: {targetUrl}, Request Body: {requestBody}");
        }

        private static async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            using StreamReader reader = new(request.Body, Encoding.UTF8, true, 1024, true);
            string body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private async Task LogResponseDetails(HttpContext context, Stopwatch stopwatch, string responseContent)
        {
            var elapsedTime = stopwatch.Elapsed;
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation($"Response Status Code: {statusCode}");
            _logger.LogInformation($"Response Content: {responseContent}");
            _logger.LogInformation($"Elapsed Time: {elapsedTime.TotalMilliseconds} ms");
        }

        private static async Task<string> GetResponseBody(Stream responseBodyStream)
        {
            using StreamReader reader = new(responseBodyStream, Encoding.UTF8, true, 1024, true);
            return await reader.ReadToEndAsync();
        }
    }
}
