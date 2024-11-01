using Dapper;
using DataAccess;
using System.Text;

namespace CodeValueREST.Features.LoggingMiddleware;

public class ResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Keep a reference to the original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a new memory stream to hold the response
        using(var responseBody = new MemoryStream())
        {
            // Replace the response body stream with our memory stream
            context.Response.Body = responseBody;

            // Continue down the middleware pipeline
            await _next(context);

            // Reset the memory stream position to the beginning
            responseBody.Seek(0, SeekOrigin.Begin);

            // Read the response body from the memory stream
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();

            // Calculate the response size
            var responseSize = Encoding.UTF8.GetByteCount(responseText);

            // Get the response code and timestamp
            var responseCode = context.Response.StatusCode;
            var timestamp = DateTime.UtcNow;

            // Log the response details if available
            if(context.Items.TryGetValue("RequestLogId", out var requestIdObj) &&
                requestIdObj is Guid requestId)
            {
                // Resolve the IDbConnection from the request services
                var connector = context.RequestServices.GetRequiredService<IDbConnector>();
                using var connection = connector.Connect();

                var sql = @"
                    UPDATE request_log
                    SET response_code = @ResponseCode,
                        response_size = @ResponseSize,
                        response_time = @ResponseTime
                    WHERE id = @Id
                ";

                await connection.ExecuteAsync(sql, new
                {
                    Id = requestId,
                    ResponseCode = responseCode,
                    ResponseSize = responseSize,
                    ResponseTime = timestamp
                });
            }

            // Reset the memory stream position again before copying
            responseBody.Seek(0, SeekOrigin.Begin);

            // Copy the contents of the memory stream to the original response body stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}

