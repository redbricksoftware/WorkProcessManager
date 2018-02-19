using System;
using Newtonsoft.Json;

namespace RedBrickSoftware.ClassLibrary
{
    public class RabbitMQQueue
    {
        [JsonProperty("messages")]
        public int Messages { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        public RabbitMQQueue()
        {
        }

        public override string ToString()
        {
            return string.Format("{{'messages': '{0}', 'name': '{1}'}}", Messages, Name);

        }
    }
}
