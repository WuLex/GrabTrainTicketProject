using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace GrabTrainTicket.Hubs
{
    public class QueueHub : Hub
    {
        private readonly IConnection _rabbitConnection;
        private static int queueNumber = 0;
        private static object lockObj = new object();
        private static ConcurrentQueue<string> ticketQueue = new ConcurrentQueue<string>();
        private static int currentNumber = 0;

        //public async Task JoinQueue()
        //{
        //    currentNumber++;
        //    ticketQueue.Enqueue(Context.ConnectionId);
        //    await Clients.Caller.SendAsync("ReceiveQueueNumber", currentNumber);
        //    await Clients.Caller.SendAsync("ReceiveMessage", "加入排队成功");
        //}

        public QueueHub(IConnection rabbitConnection)
        {
            _rabbitConnection = rabbitConnection;
        }

        public async Task JoinQueue()
        {
            // 处理用户进入排队请求
            // 生成排队号码，保存到数据库
            string queueNumber = GenerateQueueNumber();

            // 将排队号码发送给客户端
            await Clients.Caller.SendAsync("queueUpdate", new { queueNumber });
            await Clients.Caller.SendAsync("ReceiveMessage", "加入排队成功");

            // 将排队号码发布到RabbitMQ队列中，以供后续处理
            using (var channel = _rabbitConnection.CreateModel())
            {
                channel.QueueDeclare("ticketQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var message = Encoding.UTF8.GetBytes(queueNumber);
                channel.BasicPublish("", "ticketQueue", null, message);
            }
        }

        private string GenerateQueueNumber()
        {
            // 生成排队号码的逻辑，可根据实际需求实现
            // 示例中使用简单的递增
            // 注意：这里的逻辑只是示例，实际应用中可能需要更复杂的算法
            // 可以考虑使用分布式锁来确保排队号码的唯一性和正确递增
            // 也可以将排队号码存储到数据库中，实现更灵活的管理和查询

            // 假设数据库中有一个名为"queue"的表，包含一个自增字段"number"
            // 可以使用数据库操作工具（如Entity Framework）来操作数据库

            // 示例中假设使用一个静态变量来模拟数据库记录的递增
            lock (lockObj)
            {
                queueNumber++;
                return queueNumber.ToString();
            }
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "连接成功");
            await base.OnConnectedAsync();
        }
         
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (!string.IsNullOrEmpty(Context.ConnectionId))
            {
                ticketQueue = new ConcurrentQueue<string>(ticketQueue.ToArray());
                ticketQueue.TryDequeue(out string connectionId);
                while (!string.IsNullOrEmpty(connectionId))
                {
                    if (connectionId == Context.ConnectionId)
                    {
                        break;
                    }
                    ticketQueue.TryDequeue(out connectionId);
                }
                ticketQueue = new ConcurrentQueue<string>(ticketQueue.ToArray());
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
