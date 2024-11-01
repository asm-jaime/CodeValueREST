using Dapper;
using DataAccess;
using System.Text;

namespace CodeValueREST.Features.LoggingMiddleware;

public class ResponseLoggingMiddleware(RequestDelegate next)
{
    #region SQL Statements

    private readonly string _updateResponseInfoIntoRequestLog = @"
                    UPDATE request_log
                    SET response_code = @ResponseCode,
                        response_size = @ResponseSize,
                        response_time = @ResponseTime
                    WHERE id = @Id
                ";

    #endregion

    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
        var responseSize = Encoding.UTF8.GetByteCount(responseText);
        var responseCode = context.Response.StatusCode;
        var timestamp = DateTime.UtcNow;

        if(context.Items.TryGetValue("RequestLogId", out var requestIdObj) && requestIdObj is Guid requestId)
        {
            var connector = context.RequestServices.GetRequiredService<IDbConnector>();
            using var connection = connector.Connect();
            await connection.ExecuteAsync(_updateResponseInfoIntoRequestLog, new
            {
                Id = requestId,
                ResponseCode = responseCode,
                ResponseSize = responseSize,
                ResponseTime = timestamp
            });
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }
}

