using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderWorker.API.Domain;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace OrderWorker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        public IActionResult InsertOrder(Order order)
        {
            try
            {
                #region Inserir na fila 
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "orderQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    // converte a mensagem para o formato json
                    string message = JsonSerializer.Serialize(order);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "orderQueue",
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }

                #endregion
                return Accepted();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentar criar um novo pedido", ex);
                return new StatusCodeResult(500);

            }

        }
    }
}
