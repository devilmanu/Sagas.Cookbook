using GreenPipes;
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
    .AddZipkinExporter());
builder.Services.Configure<ZipkinExporterOptions>(builder.Configuration.GetSection("Zipkin"));


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
        c.UseConcurrentMessageLimit(1);
        //c.UseScheduledRedelivery(r => r.Intervals(TimeSpan.FromSeconds(10)));
        c.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(10)));
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    var contextPizzaButtSagas = scope.ServiceProvider.GetService<PizzaButtDbContext>();
    contextPizzaButt.Database.Migrate();
    contextPizzaButtSagas.Database.Migrate();
}


app.Run();
