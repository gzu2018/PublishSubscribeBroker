using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Broker
{
    public class SocketServer
    {
        private readonly Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        // used for WebSocket handshake
        private string _guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private SHA1 _sha1 = SHA1CryptoServiceProvider.Create();

        public void StartServer(IPAddress ip, int port)
        {
            Console.WriteLine($"Socket Server Running on http://{ip}:{port}");
            _serverSocket.Bind(new IPEndPoint(ip, port));
            _serverSocket.Listen(120);
            _serverSocket.BeginAccept(null, 0, OnAccept, null);
        }

        private void OnAccept(IAsyncResult result)
        {
            try
            {
                var receivedSocket = _serverSocket.EndAccept(result);
                Console.WriteLine("Subscriber connected");
                PerformHandshake(receivedSocket);

                var newSubscriber = new Subscriber(receivedSocket);
                var id = Broker.AddSubscriberToMap(newSubscriber);
                newSubscriber.SendMessage(id.ToString());
            }
            catch (SocketException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
            }
            catch (IndexOutOfRangeException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
            }
            finally
            {
                if (_serverSocket != null && _serverSocket.IsBound)
                    _serverSocket.BeginAccept(null, 0, OnAccept, null);
            }
        }

        private void PerformHandshake(Socket socket)
        {
            var buffer = new byte[1024];
            var amountReceived = socket.Receive(buffer);
            var headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, amountReceived);

            var key = headerResponse.Replace("ey:", "`")
                .Split('`')[1]
                .Replace("\r", "").Split('\n')[0]
                .Trim();
            var accKey = AcceptKey(ref key);
            var response = "HTTP/1.1 101 Switching Protocols" + "\r\n"
                                                              + "Upgrade: websocket" + "\r\n"
                                                              + "Connection: Upgrade" + "\r\n"
                                                              + "Sec-WebSocket-Accept: " + accKey + "\r\n" + "\r\n";
            socket.Send(System.Text.Encoding.UTF8.GetBytes(response));
        }

        private string AcceptKey(ref string key)
        {
            var longKey = key + _guid;
            var hashBytes = ComputeHash(longKey);
            return Convert.ToBase64String(hashBytes);
        }

        private byte[] ComputeHash(string str)
        {
            return _sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
        }
    }
}
