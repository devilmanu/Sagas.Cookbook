using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenTelemetry
builder.Services.AddOpenTelemetryTracing((b) => b
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Metrics"))
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddMassTransitInstrumentation()
    .AddSqlClientInstrumentation(o =>
    {
        o.EnableConnectionLevelAttributes = true;
        o.SetDbStatementForText = true;
    })
    .AddZipkinExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
