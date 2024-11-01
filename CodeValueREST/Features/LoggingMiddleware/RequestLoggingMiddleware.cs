using Dapper;
using DataAccess;
using System.Text;

namespace CodeValueREST.Features.LoggingMiddleware;

public class RequestLoggingMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
{
    private readonly RequestDelegate _next = next;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InvokeAsync(HttpContext context)
    {
        var scope = _serviceProvider.CreateScope();
        context.Items["RequestScope"] = scope;

        var connector = scope.ServiceProvider.GetService<IDbConnector>() ?? throw new ApplicationException($"can not get connector in {typeof(RequestLoggingMiddleware)}");

        var id = Guid.NewGuid();
        context.Items["RequestLogId"] = id;

        var timestamp = DateTime.UtcNow;

        var request = context.Request;
        var requestUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

        request.EnableBuffering();
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        var requestSize = Encoding.UTF8.GetByteCount(requestBody);

        using var connection = connector.Connect();

        var sql = @"
            INSERT INTO request_log (id, request_time, response_time, request_url, response_code, request_size, response_size)
            VALUES (@Id, @RequestTime, null, @RequestUrl, 0,  @RequestSize, 0)
        ";

        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            RequestTime = timestamp,
            RequestUrl = requestUrl,
            RequestSize = requestSize
        });

        await _next(context);
    }
}