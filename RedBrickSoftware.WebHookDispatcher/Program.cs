using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RedBrickSoftware.WebHookDispatcher
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            rabbitstuff();
        }

        static async Task<Uri> PostAsync()
        {
            object product = new object();
            HttpResponseMessage response = await client.PostAsJsonAsync("api/products", product);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        static async Task<object> GetAsync(string path)
        {
            object product = new object();
            //Product product = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                var resp = await response.Content.ReadAsAsync<object>();
            }
            return product;
        }


        static void rabbitstuff()
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
                    string queueName = "webhook";

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

                        Console.WriteLine("message: " + body);

                        channel.BasicAck(result.DeliveryTag, false);
                    }
                }

            }
        }
    }
}
