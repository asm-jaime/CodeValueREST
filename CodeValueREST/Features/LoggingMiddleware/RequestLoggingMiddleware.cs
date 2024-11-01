using Dapper;
using DataAccess;
using System.Text;

namespace CodeValueREST.Features.LoggingMiddleware;

public class RequestLoggingMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
{
    #region SQL Statements

    private readonly string _insertRequestInfoIntoRequestLog = @"
            INSERT INTO request_log (id, request_time, response_time, request_url, response_code, request_size, response_size)
            VALUES (@Id, @RequestTime, null, @RequestUrl, 0,  @RequestSize, 0)
        ";

    #endregion

    private readonly RequestDelegate _next = next;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InvokeAsync(HttpContext context)
    {
        var scope = _serviceProvider.CreateScope();

        var id = Guid.NewGuid();
        context.Items["RequestLogId"] = id;

        var timestamp = DateTime.UtcNow;
        var request = context.Request;
        var requestUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        request.EnableBuffering();
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;
        var requestSize = Encoding.UTF8.GetByteCount(requestBody);

        var connector = scope.ServiceProvider.GetService<IDbConnector>() ?? throw new ApplicationException($"can not get connector in {typeof(RequestLoggingMiddleware)}");
        using var connection = connector.Connect();

        await connection.ExecuteAsync(_insertRequestInfoIntoRequestLog, new
        {
            Id = id,
            RequestTime = timestamp,
            RequestUrl = requestUrl,
            RequestSize = requestSize
        });

        await _next(context);
    }
}