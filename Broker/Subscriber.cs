using System;
using System.Net.Sockets;
using System.Text;

namespace Broker
{
    public class Subscriber
    {
        private readonly Socket _socket;

        public Subscriber(Socket socket)
        {
            _socket = socket;
        }

        public void SendMessage(string messageContent)
        {
            try
            {
                var dataBuffer = Encoding.ASCII.GetBytes(messageContent);
                _socket.BeginSend(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallback),
                    null);
            }
            catch (SocketException exp)
            {
                Console.WriteLine($"[ERR] {exp.Message}");
            }

        }

        public bool IsSocketStillConnected()
        {
            bool part1 = _socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (_socket.Available == 0);
            return !((part1 && part2) || !_socket.Connected);
        }

        private void SendCallback(IAsyncResult AR)
        {
            _socket.EndSend(AR);
        }
    }
}