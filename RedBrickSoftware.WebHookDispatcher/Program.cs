using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RedBrickSoftware.ClassLibrary;

namespace RedBrickSoftware.WebHookDispatcher
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");




            getDataFromQueue();

        
        }



        static async Task<Uri> GetWork(WorkTypeEnumeration workType)
        {
            //TODO this

            object product = new object();
            HttpResponseMessage response = await client.PostAsJsonAsync("api/products", product);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
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


        static void getDataFromQueue()
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

                        object data = Deserialize<object>(body);

                        Console.WriteLine("message: " + data);

                        channel.BasicAck(result.DeliveryTag, false);
                    }
                }

            }
        }

        static T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }
    }
}
