using GrabTrainTicket.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace GrabTrainTicket.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConnection _rabbitConnection;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConnection rabbitConnection)
        {
            _logger = logger;
            _rabbitConnection = rabbitConnection;
        }

        //[HttpGet]
        //public IActionResult StartProcessingTickets()
        //{
        //    string result = string.Empty;
        //    using (var channel = _rabbitConnection.CreateModel())
        //    {
        //        channel.QueueDeclare("ticketQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

        //        var consumer = new EventingBasicConsumer(channel);
        //        consumer.Received += (model, ea) =>
        //        {
        //            var body = ea.Body.ToArray();
        //            var queueNumber = Encoding.UTF8.GetString(body);
        //            // 处理排队号码，购票逻辑，可根据实际需求实现 
        //            channel.BasicAck(ea.DeliveryTag, multiple: false);

        //            result = $"处理排队号码{queueNumber}成功";
        //        };
        //        channel.BasicConsume("ticketQueue", autoAck: false, consumer);
        //    }
        //    //return Content(result);
        //    return Json(result);
        //}


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}