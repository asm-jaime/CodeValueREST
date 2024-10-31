using Dapper;
using System.Data;
using System.Text;

public class ResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        using(var responseBody = new MemoryStream())
        context.Response.Body = responseBody;

        await _next(context);

        var timestamp = DateTime.UtcNow;
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var responseSize = Encoding.UTF8.GetByteCount(responseText);

        var responseCode = context.Response.StatusCode;

        if(context.Items.TryGetValue("RequestLogId", out var requestIdObj) && requestIdObj is Guid requestId)
        {
            var sql = @"
                    UPDATE request_log
                    SET response_code = @ResponseCode,
                        response_size = @ResponseSize
                    WHERE id = @Id
                ";

            var connection = context.RequestServices.GetRequiredService<IDbConnection>();
            await connection.ExecuteAsync(sql, new
            {
                Id = requestId,
                ResponseCode = responseCode,
                ResponseSize = responseSize,
                ResponseTime = timestamp
            });
        }

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        await context.Response.Body.CopyToAsync(originalBodyStream);
    }
}
