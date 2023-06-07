using GrabTrainTicket.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace GrabTrainTicket.Hubs
{

    /// <summary>
    /// 未使用此类
    /// </summary>
    public class TicketHub : Hub
    {
        private static ConcurrentQueue<string> ticketQueue = new ConcurrentQueue<string>();
        private static Dictionary<string, Ticket> tickets = new Dictionary<string, Ticket>();

        public async Task JoinQueue()
        {
            if (tickets.Count == 0)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "当前无可用票务");
                return;
            }

            string ticketId = GetAvailableTicketId();
            if (string.IsNullOrEmpty(ticketId))
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "抱歉，票已售罄");
                return;
            }

            Ticket ticket = tickets[ticketId];
            ticket.IsAvailable = false;

            int currentNumber = GetCurrentQueueNumber();
            ticketQueue.Enqueue(Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceiveQueueNumber", currentNumber);
            await Clients.Caller.SendAsync("ReceiveMessage", "加入排队成功");
            await Clients.Caller.SendAsync("ReceiveMessage", "您的排队号码是：" + currentNumber);

            await SaveTicketsToFile(); // 数据持久化
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

        private string GetAvailableTicketId()
        {
            foreach (var ticket in tickets.Values)
            {
                if (ticket.IsAvailable)
                {
                    return ticket.TicketId;
                }
            }
            return null;
        }

        private int GetCurrentQueueNumber()
        {
            int currentNumber = 0;
            foreach (var connectionId in ticketQueue)
            {
                currentNumber++;
                if (connectionId == Context.ConnectionId)
                {
                    break;
                }
            }
            return currentNumber;
        }

        private async Task LoadTicketsFromFile()
        {
            try
            {
                string filePath = "tickets.json";
                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    tickets = JsonConvert.DeserializeObject<Dictionary<string, Ticket>>(json);
                }
                else
                {
                    // 初始化票务数据
                    tickets = new Dictionary<string, Ticket>
                            {
                            { "T001", new Ticket { TicketId = "T001", IsAvailable = true } },
                            { "T002", new Ticket { TicketId = "T002", IsAvailable = true } },
                            { "T003", new Ticket { TicketId = "T003", IsAvailable = true } }
                            };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading tickets: " + ex.Message);
            }
        }

        private async Task SaveTicketsToFile()
        {
            try
            {
                string filePath = "tickets.json";
                string json = JsonConvert.SerializeObject(tickets);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving tickets: " + ex.Message);
            }
        }
    }
}