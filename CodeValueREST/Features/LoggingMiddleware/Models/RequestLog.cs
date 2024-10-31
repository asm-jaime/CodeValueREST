namespace CodeValueREST.Features.LoggingMiddleware.Models;

public class RequestLog
{
    public Guid Id { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime ResponseTime { get; set; }
    public string RequestUrl { get; set; }
    public int RequestSize { get; set; }
    public int ResponseCode { get; set; }
    public int ResponseSize { get; set; }
}
