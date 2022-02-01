using PizzaButt.Notifications.Infrastructure;
using PizzaButt.Notifications.Services;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSignalR();
builder.Services.AddTransient<IOrderService, OrderService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapHub<OrdersHub>("/ordersHub");
});

app.Run();
