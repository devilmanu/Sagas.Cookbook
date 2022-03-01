using GreenPipes;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PizzaButt.Orders.Consumers;
using PizzaButt.Orders.Infrastructure.Persistence;
using PizzaButt.Orders.Infrastructure.Sagas;
using PizzaButt.Orders.Services.Oders;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PizzaButtDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Pizzabutt")));

// OpenTelemetry
builder.Services.AddOpenTelemetryTracing((b) => b
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Zipkin:ServiceName")))
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddMassTransitInstrumentation()
    .AddSqlClientInstrumentation(o =>
    {
        o.EnableConnectionLevelAttributes = true;
        o.SetDbStatementForText = true;
        o.RecordException = true;
    })
    .AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri("http://localhost:4317/");
    }));

builder.Services.Configure<ZipkinExporterOptions>(builder.Configuration.GetSection("Zipkin"));

builder.Services.AddHttpClient("PizzaButt.Metrics", http =>
{
    http.BaseAddress = new Uri("https://localhost:7056");
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IOrdersService, OrdersService>();
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<OrderSubmitedConsumer>();
    x.AddConsumer<OrderAcceptedConsumer>();
    x.AddConsumer<OrderShippedConsumer>();
    x.AddConsumer<OrderFinishedConsumer>();
    x.AddConsumer<OrderFailedConsumer>();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>(c =>
    {
        //c.UseConcurrentMessageLimit(1);
        //c.UseConcurrencyLimit(1);
        //c.UseRateLimit(1);
        //c.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromSeconds(10)));
        c.UseInMemoryOutbox();

    })
    .InMemoryRepository();
    //.EntityFrameworkRepository(r =>
    //{
    //    r.ConcurrencyMode = ConcurrencyMode.Pessimistic; // or use Optimistic, which requires RowVersion
    //    r.AddDbContext<DbContext, PizzaButtSagasDbContext>((provider, conf) =>
    //    {
    //        conf.UseSqlServer(builder.Configuration.GetConnectionString("Pizzabuttsagas"), m =>
    //        {
    //            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
    //            //m.MigrationsHistoryTable($"__{nameof(PizzaButtSagasDbContext)}");
    //        });
    //    });
    //});

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", 5672, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddMassTransitHostedService();


var configureProblemDetails = (ProblemDetailsOptions options) =>
{
    // Only include exception details in a development environment. There's really no nee
    // to set this as it's the default behavior. It's just included here for completeness :)
    options.IncludeExceptionDetails = (ctx, ex) => true;

    // You can configure the middleware to re-throw certain types of exceptions, all exceptions or based on a predicate.
    // This is useful if you have upstream middleware that needs to do additional handling of exceptions.
    options.Rethrow<NotSupportedException>();

    // This will map NotImplementedException to the 501 Not Implemented status code.
    options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

    // This will map HttpRequestException to the 503 Service Unavailable status code.
    options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

    // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
    // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
    //options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);

    options.MapToStatusCode<KeyNotFoundException>(StatusCodes.Status404NotFound);

    options.MapToStatusCode<ResourceFoundException>(StatusCodes.Status303SeeOther);
};



builder.Services.AddProblemDetails(configureProblemDetails);
builder.Services.AddControllers()
        .AddProblemDetailsConventions()
        .AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseProblemDetails();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    var path = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent;
    var file = Path.Combine(path.ToString(), @"docker-compose.yml");
    var svc = new Ductus.FluentDocker.Builders.Builder()
                      .UseContainer()
                      .UseCompose()
                      .FromFile(file)
                      .RemoveOrphans()
                      .WaitForPort("sql", "1433")
                      .Build().Start();

}

if (builder.Configuration.GetSection("Database:ApplyMigrations").Get<bool>())
{
    using var scope = app.Services.CreateScope();
    var contextPizzaButt = scope.ServiceProvider.GetService<PizzaButtDbContext>();
    contextPizzaButt.Database.Migrate();
}

app.Run();
