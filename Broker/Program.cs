using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Nancy.Hosting.Self;

namespace Broker
{
    public class Program
    {
        static void Main(string[] args)
        {
            // make sure you create a reservation
            // netsh http add urlacl url=http://+:8888/ user=DOMAIN\username

            using var RESTServer = new NancyHost(new Uri("http://localhost:8888"));
            RESTServer.Start();
            Console.WriteLine("Rest API Server Running on http://localhost:8888");

            var socketServer = new SocketServer();
            socketServer.StartServer(IPAddress.Loopback, 6666);

            RemoveInactiveSubscriberConnectionTask(10);

            Console.ReadLine();
        }

        private static async Task RemoveInactiveSubscriberConnectionTask(int delayInSeconds)
        {
            while (true)
            {
                Task.Run(() => Broker.RemoveInactiveSubscriberConnections());
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
            }
        }
    }
}
