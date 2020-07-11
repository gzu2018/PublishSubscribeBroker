using System;
using System.Net;
using System.Net.Sockets;

namespace Broker
{
    public class SocketServer
    {
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void StartServer(IPAddress ip, int port)
        {
            Console.WriteLine($"Socket Server Running on http://{ip}:{port}");
            _serverSocket.Bind(new IPEndPoint(ip, port));
            _serverSocket.Listen(20);
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            try
            {
                var receivedSocket = _serverSocket.EndAccept(AR);
                Console.WriteLine("Subscriber connected");
                var newSubscriber = new Subscriber(receivedSocket);
                var id = Broker.AddSubscriberToMap(newSubscriber);
                newSubscriber.SendMessage(id.ToString());
                _serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (SocketException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
            }
        }
    }
}
