using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RedBrickSoftware.ClassLibrary;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RedBrickSoftware.WebHookWorkQuery.Controllers
{
    [Route("api/[controller]")]
    public class GetWorkController : Controller
    {
        //static HttpClient client = new HttpClient();
        static Dictionary<string, bool> waitTimers = new Dictionary<string, bool>();

        // GET: api/values
        [HttpGet]
        public IEnumerable<RabbitMQQueue> Get()
        {

            string filter = HttpContext.Request.Query["filter"].ToString();

            if (waitTimers.ContainsKey(filter) && (waitTimers[filter]))
            {
                Response.StatusCode = 309;
                return null;
            }

            SetTimer(filter);

            List<RabbitMQQueue> queues = GetQueuesAsync(filter).Result;

            if (addDataToQueue(queues))
            {

                return queues;
            }

            Response.StatusCode = 400;
            return null;
        }

        static void SetTimer(string filter)
        {
            if (!waitTimers.ContainsKey(filter))
            {
                waitTimers.Add(filter, true);
            }
            else
            {
                waitTimers[filter] = true;
            }

            Timer aTimer = new Timer(2000);
            aTimer.Elapsed += (sender, e) => OnTimedEvent(sender, e, filter);
            aTimer.AutoReset = false;
            aTimer.Enabled = true;

        }

        static void OnTimedEvent(Object source, ElapsedEventArgs e, string filter)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                              e.SignalTime);

            if (waitTimers.ContainsKey(filter))
            {
                waitTimers[filter] = false;
            }
        }

        static async Task<List<RabbitMQQueue>> GetQueuesAsync(string filter = "")
        {
            List<RabbitMQQueue> queues = new List<RabbitMQQueue>();

            HttpClient client = new HttpClient();

            string path = "http://guest:guest@localhost:15672/api/queues";

            try
            {
                HttpResponseMessage response = await client.GetAsync(path);

                if (response.IsSuccessStatusCode)
                {
                    var httpResponseQueues = await response.Content.ReadAsAsync<List<RabbitMQQueue>>();

                    foreach (RabbitMQQueue queue in httpResponseQueues)
                    {
                        if (queue.Messages > 0 && queue.Name.IndexOf(filter) >= 0)
                        {
                            queues.Add(queue);
                        }
                    }
                }

            }
            catch (HttpRequestException ex)
            {

            }
            catch (InvalidOperationException ex)
            {

            }
            catch (ArgumentNullException ex)
            {

            }

            return queues;
        }

        static bool addDataToQueue(List<RabbitMQQueue> queues)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            factory.UserName = "guest";
            factory.Password = "guest";
            //factory.VirtualHost = vhost;
            factory.HostName = "localhost";
            factory.Port = 5672;


            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    string exchangeName = "processmanager";
                    string routingKey = "webhook";

                    IBasicProperties properties = channel.CreateBasicProperties();
                    //properties.ContentType = "text/plain";


                    foreach (RabbitMQQueue queue in queues)
                    {
                        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(queue.ToString());
                        channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
                    }
                }

            }

            return true;
        }

        static T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }

    }
}
