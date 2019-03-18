using System;

namespace DeserializeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = new StringObjectSerializer(new JsonSerializer()).Deserialize("erhejrhejr", typeof(ServiceRouteDescriptor)) as ServiceRouteDescriptor;
            Console.WriteLine("Hello World!");
        }
    }
}
