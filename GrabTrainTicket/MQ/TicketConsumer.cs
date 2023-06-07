using GrabTrainTicket.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace GrabTrainTicket.MQ
{
    public class TicketConsumer
    {
        private readonly IConnection _rabbitConnection;
        private readonly IHubContext<QueueHub> _hubContext;

        public TicketConsumer(IConnection rabbitConnection, IHubContext<QueueHub> hubContext)
        {
            _rabbitConnection = rabbitConnection;
            _hubContext = hubContext;
        }

        public void ConsumeTickets()
        {
            using (var channel = _rabbitConnection.CreateModel())
            {
                channel.QueueDeclare("ticketQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var queueNumber = Encoding.UTF8.GetString(body);

                    // 处理购票逻辑，例如请求12306接口购买车票

                    // 假设购票成功
                    bool purchaseSuccess = true;

                    if (purchaseSuccess)
                    {
                        // 发送购票成功通知给客户端
                        await _hubContext.Clients.All.SendAsync("ticketPurchased", new { queueNumber });
                    }
                    else
                    {
                        // 发送购票失败通知给客户端
                        await _hubContext.Clients.All.SendAsync("ticketPurchaseFailed", new { queueNumber });
                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume("ticketQueue", false, consumer);
            }
        }
    }
}
