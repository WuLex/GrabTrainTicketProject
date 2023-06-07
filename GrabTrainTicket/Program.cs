using GrabTrainTicket.Hubs;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
// �������������������RabbitMQ��Redis��
// ע��RabbitMQ����
var rabbitConnection = CreateRabbitConnection();
builder.Services.AddSingleton<IConnection>(rabbitConnection);
builder.Services.AddSignalR(hubOptions =>
{
    //hubOptions.EnableDetailedErrors = true;
    //// Maybe I don��t need those, but connection is dropping very frequently with defaults.
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
    // �������·������
});
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
IConnection CreateRabbitConnection()
{
    // ����RabbitMQ���ӵ��߼�
    // ����ֻ��һ��ʾ��������Ҫ����ʵ���������ʵ��
    var factory = new ConnectionFactory()
    {
        HostName = "192.168.0.106",
        UserName = "wulex",
        Password = "***********"
    };

    return factory.CreateConnection();
}