using System;

namespace RedBrickSoftware.ClassLibrary
{
    public class WebhookProcess<T>
    {
        public string URI { get; set; }
        public int Port { get; set; }
        public HTTPMethodEnumeration HTTPMethod { get; set; }
        public T Payload { get; set; }
        public bool GuarnateeMessageOrder { get; set; }
    }
}
