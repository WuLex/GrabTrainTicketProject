using GrabTrainTicket.Hubs;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
// 添加其他服务依赖，如RabbitMQ、Redis等
// 注册RabbitMQ连接
var rabbitConnection = CreateRabbitConnection();
builder.Services.AddSingleton<IConnection>(rabbitConnection);
builder.Services.AddSignalR(hubOptions =>
{
    //hubOptions.EnableDetailedErrors = true;
    //// Maybe I don’t need those, but connection is dropping very frequently with defaults.
    //hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
    //hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(5);
    //hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(15);
    //hubOptions.MaximumReceiveMessageSize = 102400000;
});
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<QueueHub>("/queueHub");
    // 添加其他路由配置
});
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
IConnection CreateRabbitConnection()
{
    // 创建RabbitMQ连接的逻辑
    // 这里只是一个示例，你需要根据实际需求进行实现
    var factory = new ConnectionFactory()
    {
        HostName = "192.168.0.106",
        UserName = "wulex",
        Password = "***********"
    };

    return factory.CreateConnection();
}