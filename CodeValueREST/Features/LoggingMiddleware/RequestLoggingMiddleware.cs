using Dapper;
using System.Data;
using System.Text;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var id = Guid.NewGuid();
        context.Items["RequestLogId"] = id;

        var timestamp = DateTime.UtcNow;

        var request = context.Request;
        var requestUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        request.EnableBuffering();
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        var requestSize = Encoding.UTF8.GetByteCount(requestBody);

        using(var scope = context.RequestServices.CreateScope())
        {
            var connection = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            var sql = @"
            INSERT INTO request_log (id, timestamp, request_url, request_size, response_code, response_size)
            VALUES (@Id, @Timestamp, @RequestUrl, @RequestSize, 0, 0)
        ";

            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                RequestTime = timestamp,
                RequestUrl = requestUrl,
                RequestSize = requestSize
            });
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
