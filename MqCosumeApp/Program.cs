using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

#region 程序主体
// 创建依赖注入容器
var serviceProvider = CreateServiceProvider();
// 获取注入的RabbitMQ连接实例
IConnection _rabbitConnection = serviceProvider.GetService<IConnection>();

// 监听队列
StartProcessingTickets();
// 阻止控制台应用程序退出
Console.WriteLine("按任意键退出...");
Console.ReadKey(); 
#endregion

ServiceProvider CreateServiceProvider()
{
    var services = new ServiceCollection();

    // 注册RabbitMQ连接
    var rabbitConnection = CreateRabbitConnection();
    services.AddSingleton<IConnection>(rabbitConnection);

    return services.BuildServiceProvider();
}

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

void StartProcessingTickets()
{
    using (var channel = _rabbitConnection.CreateModel())
    {
        channel.QueueDeclare("ticketQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var queueNumber = Encoding.UTF8.GetString(body);
            // 处理排队号码，购票逻辑，可根据实际需求实现
            channel.BasicAck(ea.DeliveryTag, multiple: false);

            Console.WriteLine($"处理排队号码{queueNumber}成功");
        };
        channel.BasicConsume("ticketQueue", autoAck: false, consumer);

        // 通过while循环持续监听消息队列
        while (true)
        {
            Thread.Sleep(1000); // 控制循环速率
        }
    }
}