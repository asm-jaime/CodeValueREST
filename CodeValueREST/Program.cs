using CodeValueREST.Features.CodeValues.Handlers;
using CodeValueREST.Features.CodeValues.Providers;
using CodeValueREST.Features.LoggingMiddleware;
using DataAccess;
using DataAccess.Postgres;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnector>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new PostgresDbConnector(connectionString);
});

builder.Services.AddTransient<CodeValueProvider>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetCodeValuesQueryHandler).Assembly)
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ResponseLoggingMiddleware>();

app.MapControllers();

app.Run();

