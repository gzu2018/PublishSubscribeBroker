using System;
using System.Net;
using Nancy.Hosting.Self;

namespace Broker
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Create a reservation for Nancy or else Rest server will not start up
            // Windows CMD: netsh http add urlacl url=http://+:8888/ user=DOMAIN\username

            using var restServer = new NancyHost(new Uri("http://localhost:8888"));
            restServer.Start();
            Console.WriteLine("Rest API Server Running on http://localhost:8888");

            var socketServer = new SocketServer();
            socketServer.StartServer(IPAddress.Any, 8080);

            Broker.PeriodicallyRemoveInactiveSubscribersTask(30, false);

            Console.ReadLine();
        }
    }
}
