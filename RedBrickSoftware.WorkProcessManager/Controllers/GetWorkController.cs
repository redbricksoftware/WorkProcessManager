using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RedBrickSoftware.ClassLibrary;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RedBrickSoftware.WorkProcessManager.Controllers
{
    [Route("api/[controller]")]
    public class GetWorkController : Controller
    {
        // GET: api/values
        [HttpGet]
        //public IEnumerable<string> Get()
        public object Get()
        {
            string filter = HttpContext.Request.Query["filter"].ToString();

            var factory = new ConnectionFactory() { HostName = "localhost" };

            factory.UserName = "guest";
            factory.Password = "guest";
            //factory.VirtualHost = vhost;
            factory.HostName = "localhost";
            factory.Port = 5672;

            RabbitMQQueue data = new RabbitMQQueue();

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //string queueName = "webhook";
                    string queueName = filter;

                    //noAck=false requires explicit acknowledgement of the message
                    bool noAck = false;
                    BasicGetResult result = channel.BasicGet(queueName, noAck);


                    if (result == null)
                    {
                        // No message available at this time.
                        Console.WriteLine("no message");
                    }
                    else
                    {
                        IBasicProperties props = result.BasicProperties;
                        byte[] body = result.Body;

                        data = Deserialize<RabbitMQQueue>(body);

                        Console.WriteLine("message: " + data.ToString());

                        channel.BasicAck(result.DeliveryTag, false);
                        return data;
                    }
                }
            }



            Response.StatusCode = 204;
            return data;
        }


        static T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }
    }
}
